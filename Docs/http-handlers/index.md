# HTTP Handlers

Corely.Common provides composable `DelegatingHandler` implementations for common cross‑cutting HTTP concerns.

## Available Handlers
- [Request / Response Logging](request-response-handler.md)
- [Error Logging](error-handler.md)
- [Static Response (Stub / Mock)](static-response-handler.md)

## Prerequisite: ILogger Registration
All handlers depend on `ILogger<T>` from Microsoft.Extensions.Logging. Ensure logging is registered before adding the handlers, otherwise DI resolution will fail.

Minimal logging setup:
```csharp
services.AddLogging(b => b.AddConsole());
```

## Usage: Logging + Error Handlers
Add detailed (opt‑in) request/response logging and non‑success error logging. This is the common production setup.
```csharp
services.AddHttpClient("observed")
    .AddHttpMessageHandler<HttpRequestResponseLoggingHandler>()  // Detailed opt‑in logging
    .AddHttpMessageHandler<HttpErrorLoggingHandler>();           // Non‑success error logging
```
Requests only include headers/body scopes when you call helper extension methods (see handler docs). Order these before retry / resiliency handlers if you want each attempt logged.

## Usage: Static Response Handler (Testing / Stubbing)
`HttpStaticResponseHandler` is intended primarily for:
- Deterministic integration / component tests (no external dependency)
- Local development while upstream services are unavailable
- Simulating success or error responses (e.g. throttling, 500s, feature toggles)

Register its options (can be a singleton or IOptions) and place it OUTER-most so matching requests short‑circuit before other handlers execute.
```csharp
services.AddSingleton(new StaticResponseHandlerOptions {
    Enabled = true,
    PathStartsWith = "/stub",      // Any request path starting with this prefix is intercepted
    StatusCode = HttpStatusCode.OK,
    ContentType = "application/json",
    ResponseBody = "{ \"example\": true }"
});

services.AddHttpClient("stubbed")
    .AddHttpMessageHandler<HttpStaticResponseHandler>()          // Short‑circuit (when path matches)
    .AddHttpMessageHandler<HttpRequestResponseLoggingHandler>()  // Still can log intercepted call metadata
    .AddHttpMessageHandler<HttpErrorLoggingHandler>();
```
Disable (set `Enabled = false`) or remove this handler in production unless you explicitly need stub behavior.

## Ordering Guidance (outer → inner)
1. Static / short‑circuit handlers
2. Diagnostics (request / response logging)
3. Error logging & resiliency (e.g. retries / Polly)

See each handler page for detailed features and configuration.
