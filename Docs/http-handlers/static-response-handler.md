# HTTP Static Response Handler

`HttpStaticResponseHandler` conditionally short?circuits outbound HTTP calls and returns a predefined (stub) response without invoking the inner handler. Useful for tests, local development, offline scenarios, or temporarily isolating failing upstream systems.

## Features
- Path prefix matching (`PathStartsWith`, case?insensitive)
- Enable/disable toggle (`Enabled`)
- Configure status code, content type & body
- No network / inner handler invocation when intercepted
- Logs an information line when a request is intercepted
- Very low overhead when not matching (falls through immediately)

## Options (`StaticResponseHandlerOptions`)
| Property | Default | Description |
|----------|---------|-------------|
| `Enabled` | `true` | Master switch ? if false acts as a transparent pass?through |
| `PathStartsWith` | `/` | Absolute path prefix to match (e.g. `/stub/api`) |
| `StatusCode` | `200 OK` | Returned HTTP status code |
| `ContentType` | `application/json` | MIME type of the stubbed content |
| `ResponseBody` | `{}` | Body payload (UTF?8) |

## Registration Example
```csharp
// Register handler. Some implementations of AddHttpMessageHandler don't do this internally
services.TryAddTransient<HttpStaticResponseHandler>();

// Options (could also use IOptions<StaticResponseHandlerOptions>)
services.AddSingleton(new StaticResponseHandlerOptions
{
    Enabled = true,
    PathStartsWith = "/external/test",
    StatusCode = HttpStatusCode.Accepted,
    ContentType = "application/json",
    ResponseBody = "{\"ok\":true}"
});

services.AddHttpClient("stubbed")
    .AddHttpMessageHandler<HttpStaticResponseHandler>();
```

## Usage
```csharp
var client = httpClientFactory.CreateClient("stubbed");
var resp = await client.GetAsync("https://api.example.com/external/test?id=1");
// -> Returns predefined response without performing a real HTTP call.
```

If the request path does not start with the configured prefix (or `Enabled` is false) the request continues down the normal handler chain.

## Typical Scenarios
- Replacing an unstable third?party API during development
- Deterministic integration tests without wiremock/proxy infra
- Simulating error responses (e.g. set `StatusCode = HttpStatusCode.TooManyRequests`)
- Offline demo environments

## Ordering Guidance
Place as early (outermost) as possible so that downstream handlers (logging, retry, auth) are skipped when interception occurs:
1. Static / stub handlers (this)
2. Logging / diagnostics handlers
3. Error / resiliency / retry handlers

## Log Output
On intercept:
```
StaticResponseHandler intercepted {Method} {Uri} -> returning stubbed {StatusCode}
```
Trace timing (always when intercepting):
```
HTTP HttpStaticResponseHandler (intercept) evaluated in {ElapsedMs} ms
```

## Pass?Through Behavior
When not matched it performs zero allocations beyond simple checks and delegates to the inner handler ? safe to leave registered even when rarely used.
