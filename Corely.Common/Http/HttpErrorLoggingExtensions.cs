using Microsoft.Extensions.Logging;

namespace Corely.Common.Http;

public static class HttpErrorLoggingExtensions
{
    extension(HttpRequestMessage request)
    {
        public HttpRequestMessage SetErrorLogLevel(LogLevel level)
        {
            request.Options.Set(new HttpRequestOptionsKey<LogLevel>(HttpErrorLoggingConstants.ERROR_LOG_LEVEL_OPTION), level);
            return request;
        }

        internal bool TryGetErrorLogLevel(out LogLevel level)
        {
            return request.Options.TryGetValue(new HttpRequestOptionsKey<LogLevel>(HttpErrorLoggingConstants.ERROR_LOG_LEVEL_OPTION), out level);
        }
    }
}
