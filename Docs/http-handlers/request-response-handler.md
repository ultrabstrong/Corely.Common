# HTTP Request / Response Logging Handler

`HttpRequestResponseLoggingHandler` provides structured, opt-in logging of HTTP request & response metadata, headers, and (optionally transformed) bodies. It aims to balance observability with safety & performance.

## Features
- Opt-in per request for detailed request logging (`EnableRequestDetailLogging()`)
- Opt-in per request for detailed response logging (`EnableResponseDetailLogging()`)
- Always logs basic: `HTTP {Method} {Uri} responded {StatusCode} in {ElapsedMs} ms`
  - (Only when response logging NOT explicitly enabled for that request)
- Redacts sensitive headers automatically (Authorization, Cookies, API keys, etc.)
- Optional JSON body field omission (replace value with `[OMITTED]`)
- Optional JSON body field truncation with suffix `...[TRUNCATED]`
- Preserves original request/response bodies (buffered safely)
- Measures and logs internal logging overhead at TRACE
- Thread-safe & allocation conscious

## Enabling Detailed Logging Per Request
Detailed request/response logging is intentionally off by default to avoid large log volumes.
```csharp
var request = new HttpRequestMessage(HttpMethod.Post, "https://example/api/doc")
{
    Content = new StringContent("{\"document_url\":\"abc\",\"name\":\"file.txt\"}")
};
request.EnableRequestDetailLogging();
request.EnableResponseDetailLogging();
```

## Omitting JSON Fields
```csharp
request.OmitRequestJsonFields("document_url", "apiKey");
request.OmitResponseJsonFields("secret", "token");
// "document_url":"[OMITTED]" in logged scope
```
Uses a regex replacement preserving whitespace around the colon. Non-JSON or unparsable text is left unchanged.

## Truncating JSON Fields
```csharp
request.TruncateRequestJsonFields(("body", 1000));
request.TruncateResponseJsonFields(("description", 120));
// Values exceeding max length become: valuePrefix + "...[TRUNCATED]"
```
Truncation only applies to string field values.

## Header Redaction
Sensitive headers become `[REDACTED]` in logged scopes:
- Authorization
- Proxy-Authorization
- Cookie / Set-Cookie
- X-Api-Key / Api-Key

## Registration
```csharp
// Register handler. Some implementations of AddHttpMessageHandler don't do this internally
services.TryAddTransient<HttpRequestResponseLoggingHandler>();

services.AddHttpClient("observed")
    .AddHttpMessageHandler<HttpRequestResponseLoggingHandler>();
```

## Usage
```csharp
var client = httpClientFactory.CreateClient("observed");
var request = new HttpRequestMessage(HttpMethod.Post, "https://svc/upload")
{
    Content = new StringContent("{\"fileName\":\"big.bin\",\"payload\":\"<base64>...\"}")
};
request.EnableRequestDetailLogging();
request.EnableResponseDetailLogging();
request.OmitRequestJsonFields("payload");
request.TruncateResponseJsonFields(("message", 200));

var response = await client.SendAsync(request);
```

## Logged Scope Data (when enabled)
- `HttpRequestHeaders` / `HttpResponseHeaders`: Dictionary<string,string>
- `HttpRequestBody` / `HttpResponseBody`: Full (or omitted / truncated) string

## Performance Notes
- Bodies are buffered once via `LoadIntoBufferAsync()` before reading
- Overhead stopwatch logged at TRACE: `HTTP HttpRequestResponseLoggingHandler logging overhead {OverheadMs} ms`
- If body read fails (stream not repeatable), it logs a debug message and proceeds

## Fallback Behavior
If detailed response logging disabled (`EnableResponseDetailLogging` not called), a single concise Information log line is written with latency only – no headers/body scopes.

## Safety
- No mutation of the underlying content streams beyond buffering
- Regex field operations wrapped in try/catch with warning fallback

## When To Use
- Debugging specific integration calls (opt-in per request)
- Capturing sanitized bodies for auditing
- Measuring latency while protecting secrets
