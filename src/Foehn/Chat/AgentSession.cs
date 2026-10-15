// Copyright (c) August 2026, devMobile Software
// 
using Foehn.Abstractions;
using Foehn.Configuration;
using Foehn.Plugins;
using MistralAi.Composite.Client;
using MistralAi.Composite.Client.Dtos.Shared;

namespace Foehn.Chat;

/// <summary>
/// Owns a single conversation with the Mistral LLM, driving the tool-calling loop:
/// send history + available tools, execute whatever tools the model asks for, feed the
/// results back, and repeat until the model returns a final answer (or the configured
/// iteration limit is hit).
/// </summary>
public sealed class AgentSession
{
   private readonly ChatCompletionClient _client;
   private readonly IToolRegistry _toolRegistry;
   private readonly FoehnOptions _options;
   private readonly ILogger<AgentSession> _logger;
   private static readonly List<ChatMessage> chatMessages = [];
   private readonly List<ChatMessage> _history = chatMessages;
   private readonly SystemMessage? _systemMessage;
   private bool _autoApproveAll;

   public AgentSession(
       ChatCompletionClient client,
       IToolRegistry toolRegistry,
       IOptions<FoehnOptions> options,
       ISystemPromptProvider systemPromptProvider,
       ILogger<AgentSession> logger)
   {
      _client = client;
      _toolRegistry = toolRegistry;
      _options = options.Value;
      _logger = logger;

      _systemMessage = new SystemMessage { Content = systemPromptProvider.SystemPrompt() };

      if (_systemMessage is not null)
      {
         _history.Add(_systemMessage);
      }
   }

   /// <summary>The conversation so far, in order (system message first, if configured).</summary>
   public IReadOnlyList<ChatMessage> History => _history;

   /// <summary>
   /// Drops every message except the system prompt, starting the conversation over.
   /// Does not reset the "auto-approve all" choice from tool-call confirmation prompts.
   /// </summary>
   public void ClearContext()
   {
      _history.Clear();

      if (_systemMessage is not null)
      {
         _history.Add(_systemMessage);
      }
   }

   /// <summary>
   /// Adds a skill to the conversation context.
   /// </summary>
   /// <param name="skillContent">The content of the skill to add</param>
   public void AddSkillToContext(string skillContent)
   {
      if (!string.IsNullOrWhiteSpace(skillContent))
      {
         _history.Add(new UserMessage { Content = $"[SKILL LOADED]: {skillContent}" });
      }
   }

   /// <summary>
   /// Sends a user message and runs the tool-calling loop to completion, returning the
   /// model's final text reply.
   /// </summary>
   public async Task<string> SendAsync(string userMessage, CancellationToken cancellationToken = default)
   {
      _history.Add(new UserMessage { Content = userMessage });

      var tools = _toolRegistry.BuildToolDefinitions();

      for( var iteration = 0; iteration < _options.MaxToolIterations; iteration++)
      {
         var request = new ChatCompletionRequest
         {
            Messages = _history,
            Tools = tools.Count > 0 ? tools : null,
            ToolChoice = tools.Count > 0 ? MistralAi.Composite.Client.Dtos.Shared.ToolChoice.Auto : null,
            Model = _options.Model
         };

         var response = await _client.CreateChatCompletionAsync(request, cancellationToken);

         if (response.Choices.Count == 0)
            throw new InvalidOperationException("Mistral API returned no choices.");
         
         var assistantMessage = response.Choices[0].Message;

         // The API requires tool_calls to be present (even empty) or content to be non-empty.
         var normalized = new AssistantMessage
         {
            Content = assistantMessage.Content,
            ToolCalls = assistantMessage.ToolCalls,
         };
         _history.Add(normalized);

         if (assistantMessage.ToolCalls is not { Count: > 0 })
            return normalized.Content?.Text ?? string.Empty;

         foreach (var toolCall in assistantMessage.ToolCalls)
         {
            if (toolCall.Function is null || string.IsNullOrEmpty(toolCall.Id))
               continue;

            var arguments = toolCall.Function.Arguments;
            var tool = _toolRegistry.Tools.FirstOrDefault(t =>
                string.Equals(t.Name, toolCall.Function.Name, StringComparison.OrdinalIgnoreCase));
            var riskLevel = tool?.RiskLevel ?? ToolRiskLevel.Undefined;

            Console.WriteLine($"  -> {toolCall.Function.Name}({arguments})");

            ToolExecutionResult result;
            if (!ConfirmToolCall(toolCall.Function.Name, riskLevel))
            {
               Console.WriteLine("  <- declined by user");
               result = ToolExecutionResult.Fail(
                   "The user declined to run this tool call. Do not retry the same action without new information or an explicit request from the user.");
            }
            else
            {
               result = await _toolRegistry.ExecuteAsync(toolCall.Function.Name, arguments, cancellationToken);
               Console.WriteLine(result.Success ? "  <- ok" : "  <- failed");
               // BHL Add settings to print the result of the tool call if it is not empty
               if (_options.PrintToolResults && !string.IsNullOrWhiteSpace(result.Result))
               {
                  Console.WriteLine("  <- result:");
                  PrintIndented(result.Result);
               }
            }

            _history.Add(new ToolMessage { ToolCallId = toolCall.Id, Content = result.Result });
         }
      }

      _logger.LogWarning("Reached the maximum of {MaxToolIterations} tool iterations without a final answer.", _options.MaxToolIterations);
      return "[Foehn stopped: reached the maximum number of tool call iterations without a final answer.]";
   }

   private static void PrintIndented(string text)
   {
      if (string.IsNullOrWhiteSpace(text))
         return;

      foreach (var line in text.Replace("\r\n", "\n").Split('\n'))
         Console.WriteLine($"     {line}");
   }

   // ReadOnly tools run without asking; anything that can change or destroy state
   // (including an unclassified/Undefined risk level) needs the user's say-so first.
   private static bool RequiresConfirmation(ToolRiskLevel riskLevel) => riskLevel != ToolRiskLevel.ReadOnly && riskLevel != ToolRiskLevel.Trusted;

   private bool ConfirmToolCall(string toolName, ToolRiskLevel riskLevel)
   {
      if (!RequiresConfirmation(riskLevel) || _autoApproveAll)
         return true;

      var warning = riskLevel switch
      {
         ToolRiskLevel.Write => "This will create or modify data on this machine.",
         ToolRiskLevel.Destructive => "This may permanently remove or overwrite data on this machine.",
         _ => "This tool did not declare a risk level, so it's being treated as potentially destructive.",
      };

      Console.WriteLine($"     [{riskLevel}] {warning}");
      Console.Write($"     Allow '{toolName}'? (y)es / (n)o / (a)ll - auto-approve every update for the rest of this session [y/N/a]: ");
      var answer = Console.ReadLine()?.Trim();

      if (string.Equals(answer, "a", StringComparison.OrdinalIgnoreCase) || string.Equals(answer, "all", StringComparison.OrdinalIgnoreCase))
      {
         _autoApproveAll = true;
         _logger.LogInformation("User chose to auto-approve all further tool calls for the rest of this session.");
         Console.WriteLine("     Auto-approving all further updates for the rest of this session.");
         return true;
      }

      return answer is { Length: > 0 } text && (text[0] == 'y' || text[0] == 'Y');
   }
}
