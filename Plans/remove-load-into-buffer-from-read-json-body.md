# Remove Unnecessary `LoadIntoBufferAsync` from `ReadJsonBodyAsync`

## Problem

`HttpResponseMessageJsonExtensions.ReadJsonBodyAsync` calls `LoadIntoBufferAsync()` before calling `ReadFromJsonAsync<T>()`:

```csharp
await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
return await response.Content.ReadFromJsonAsync<T>(options ?? DefaultJsonOptions, cancellationToken)
    .ConfigureAwait(false);
```

`LoadIntoBufferAsync` reads the entire response body into an in-memory buffer. This means the full response body exists in memory **twice** simultaneously — once in the buffer and once in the deserialized object graph — before the buffer is eventually collected.

## Why It Adds No Value Here

- **Nothing re-reads the content.** The buffer is consumed by `ReadFromJsonAsync` immediately after and never read again.
- **No raw body logging.** The `catch` block logs only the exception, not the response body, so buffering "for re-read on failure" isn't happening.
- **`ReadFromJsonAsync` is already safe on a live stream.** `System.Net.Http.Json` internally pipes the response stream directly to `JsonSerializer.DeserializeAsync`, handling chunked network reads correctly without needing the content pre-buffered.

## Proposed Fix

Remove the `LoadIntoBufferAsync` call:

```csharp
public static async Task<T?> ReadJsonBodyAsync<T>(
    this HttpResponseMessage? response,
    JsonSerializerOptions? options = null,
    CancellationToken cancellationToken = default,
    ILogger? logger = null
)
{
    try
    {
        if (response?.Content is null)
            return default;

        return await response.Content
            .ReadFromJsonAsync<T>(options ?? DefaultJsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        logger?.LogWarning(
            ex,
            "Failed to deserialize {TypeName} from response body.",
            typeof(T).Name
        );
        return default;
    }
}
```

## Impact

- Halves the peak memory used per HTTP response deserialization call.
- For large response bodies (e.g., OCR results, document extraction payloads), this can save tens of megabytes per invocation.
- No behavioral change for callers — the return type, error handling, and null-on-failure behavior are identical.

## Files to Change

- `Corely.Common/Http/HttpResponseMessageJsonExtensions.cs` — remove `LoadIntoBufferAsync` call
- `Corely.Common.UnitTests/Http/HttpResponseMessageJsonExtensionsTests.cs` — verify existing tests still pass; no new tests needed as behavior is unchanged
