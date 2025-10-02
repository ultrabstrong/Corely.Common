using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Corely.Common.Http;

public static class HttpResponseMessageJsonExtensions
{
    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

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

            await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
            return await response
                .Content.ReadFromJsonAsync<T>(options ?? DefaultJsonOptions, cancellationToken)
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
}
