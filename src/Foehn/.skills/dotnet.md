# .NET Development Standards

## Target Framework

- Use .NET 10.0 unless otherwise specified.
- Enable nullable reference types.
- Enable implicit usings.

## Solution Structure

- One solution file per application.
- Projects:
  - Api
  - Application
  - Domain
  - Infrastructure
  - Tests
- UserSecrets for apikeys, password, connection strings and other PII.

## Package Management

Prefer:
- Microsoft.Extensions.*
- OpenTelemetry
- FluentValidation
- Dapper
- FusionCache

Avoid introducing unnecessary dependencies.

## Configuration

- Use strongly typed options.
- Bind configuration through IOptions<T>.
- Never hardcode secrets.

## Dependency Injection

- Constructor injection only.
- No service locator pattern.
- All dependencies registered through extension methods.

## Async

- All I/O operations must be async.
- Pass CancellationToken everywhere possible.
- Use Async suffix for async methods.

Example:

public async Task<CustomerDto> GetAsync(
    long customerId,
    CancellationToken cancellationToken)