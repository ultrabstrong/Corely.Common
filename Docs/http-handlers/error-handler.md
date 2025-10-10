# HTTP Error Logging Handler

`HttpErrorLoggingHandler` logs non-successful HTTP responses (i.e. responses where `IsSuccessStatusCode` is false) with method, URI, status code and basic content metadata. This keeps normal (successful) traffic noise low while surfacing failures.

## Features
- Logs only failed responses (4xx / 5xx, or any non-success status)
- Includes: HTTP method, request URI, numeric status code
- Adds response `Content-Length` and `Content-Type` (when available)
- Minimal performance overhead (overhead timing logged at TRACE)
- Safe: does not attempt to read or buffer the body to avoid side-effects

## Registration
```csharp
// Register handler. Some implementations of AddHttpMessageHandler don't do this internally
services.TryAddTransient<HttpErrorLoggingHandler>();

services.AddHttpClient("with-errors")
    .AddHttpMessageHandler<HttpErrorLoggingHandler>();
```

## When To Use
- You only want detailed body/header logging for some clients (pair with the Request/Response Logging handler for richer diagnostics)
- You want a lightweight failure signal in logs without verbose success messages

## Per-request log level override
You can override the log level used for failed responses on a per-request basis via the extension:

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example/")
    .SetErrorLogLevel(LogLevel.Warning); // default is LogLevel.Error

var response = await httpClient.SendAsync(request);
```

- Default level is Error when not specified.
- Set LogLevel.None to suppress the failure log for that request.
- Only affects this handler; it does not change global logger configuration.

## Log Messages
- Error (on failure):
  `HTTP request failed. {Method} {Uri} responded {StatusCode}. ContentLength={ContentLength}, ContentType={ContentType}`
- Trace (always):
  `HTTP HttpErrorLoggingHandler logging overhead {OverheadMs} ms`
