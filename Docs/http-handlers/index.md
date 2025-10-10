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

Serilog example:
```csharp
services.AddLogging(b => b.AddSerilog(dispose: true));
```

## Usage: Logging + Error Handlers
Add detailed (opt‑in) request/response logging and non‑success error logging. This is the common production setup.
```csharp
// Register handlers. Some implementations of AddHttpMessageHandler don't do this internally
services.TryAddTransient<HttpErrorLoggingHandler>();
services.TryAddTransient<HttpRequestResponseLoggingHandler>();

services.AddHttpClient("observed")
    .AddHttpMessageHandler<HttpErrorLoggingHandler>()            // Non‑success error logging
    .AddHttpMessageHandler<HttpRequestResponseLoggingHandler>(); // Detailed opt‑in logging
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
    PathStartsWith = "/",      // matches all requests
    StatusCode = HttpStatusCode.OK,
    ContentType = "application/json",
    ResponseBody = "{ \"example\": true }"
});

// Register handlers. Some implementations of AddHttpMessageHandler don't do this internally
services.TryAddTransient<HttpErrorLoggingHandler>();
services.TryAddTransient<HttpRequestResponseLoggingHandler>();
services.TryAddTransient<HttpStaticResponseHandler>();

services.AddHttpClient("stubbed")
    .AddHttpMessageHandler<HttpErrorLoggingHandler>()
    .AddHttpMessageHandler<HttpRequestResponseLoggingHandler>()   // Still can log intercepted call metadata
    .AddHttpMessageHandler<HttpStaticResponseHandler>();          // Short‑circuit (when path matches)
```
Disable (set `Enabled = false`) or remove this handler in production unless you explicitly need stub behavior.

## Disabling Microsoft / System HTTP Logs
Registring ILogger enables default logging from Microsoft / System libraries, which may include HTTP request logs. You can configure logging filters to adjust verbosity as needed. Here is an example of how to do that with Serilog:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
```