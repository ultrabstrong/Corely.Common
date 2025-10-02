using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Corely.Common.Http;

public sealed class HttpErrorLoggingHandler(ILogger<HttpErrorLoggingHandler> logger)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var overheadSw = Stopwatch.StartNew();

        if (!response.IsSuccessStatusCode)
        {
            var contentLength = response.Content?.Headers.ContentLength;
            var contentType = response.Content?.Headers.ContentType?.ToString();
            logger.LogError(
                "HTTP request failed. {Method} {Uri} responded {StatusCode}. ContentLength={ContentLength}, ContentType={ContentType}",
                request.Method,
                request.RequestUri,
                (int)response.StatusCode,
                contentLength,
                contentType
            );
        }

        overheadSw.Stop();
        logger.LogTrace(
            "HTTP {Handler} logging overhead {OverheadMs} ms",
            nameof(HttpErrorLoggingHandler),
            overheadSw.ElapsedMilliseconds
        );

        return response;
    }
}
