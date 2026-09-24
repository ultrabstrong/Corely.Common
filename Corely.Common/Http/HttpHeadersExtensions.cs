using System.Net.Http.Headers;

namespace Corely.Common.Http;

internal static class HttpHeadersExtensions
{
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Proxy-Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key",
        "Api-Key",
    };

    extension(HttpHeaders headers)
    {
        public Dictionary<string, string> ToLoggingSnapshot(HttpContentHeaders? contentHeaders)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var h in headers)
            {
                result[h.Key] = MaskIfSensitive(h.Key, string.Join(",", h.Value));
            }

            if (contentHeaders is not null)
            {
                foreach (var h in contentHeaders)
                {
                    result[$"Content-{h.Key}"] = MaskIfSensitive(h.Key, string.Join(",", h.Value));
                }
            }
            return result;
        }
    }

    private static string MaskIfSensitive(string headerName, string value) =>
        SensitiveHeaders.Contains(headerName) ? "[REDACTED]" : value;
}
