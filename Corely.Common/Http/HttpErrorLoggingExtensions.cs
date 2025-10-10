using Microsoft.Extensions.Logging;

namespace Corely.Common.Http;

public static class HttpErrorLoggingExtensions
{
    public static HttpRequestMessage SetErrorLogLevel(this HttpRequestMessage request, LogLevel level)
    {
        request.Options.Set(new HttpRequestOptionsKey<LogLevel>(HttpErrorLoggingConstants.ERROR_LOG_LEVEL_OPTION), level);
        return request;
    }

    internal static bool TryGetErrorLogLevel(this HttpRequestMessage request, out LogLevel level)
    {
        return request.Options.TryGetValue(new HttpRequestOptionsKey<LogLevel>(HttpErrorLoggingConstants.ERROR_LOG_LEVEL_OPTION), out level);
    }
}
