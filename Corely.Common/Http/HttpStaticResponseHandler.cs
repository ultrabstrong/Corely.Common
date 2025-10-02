using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Corely.Common.Http;

public sealed class HttpStaticResponseHandler(
    ILogger<HttpStaticResponseHandler> logger,
    StaticResponseHandlerOptions options
) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (!options.Enabled || string.IsNullOrWhiteSpace(options.PathStartsWith))
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        if (!path.StartsWith(options.PathStartsWith, StringComparison.OrdinalIgnoreCase))
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var sw = Stopwatch.StartNew();

        logger.LogInformation(
            "StaticResponseHandler intercepted {Method} {Uri} -> returning stubbed {StatusCode}",
            request.Method,
            request.RequestUri,
            (int)options.StatusCode
        );

        var response = new HttpResponseMessage(options.StatusCode)
        {
            Content = new StringContent(
                options.ResponseBody ?? string.Empty,
                Encoding.UTF8,
                options.ContentType
            ),
        };

        sw.Stop();

        logger.LogTrace(
            "HTTP {Handler} (intercept) evaluated in {ElapsedMs} ms",
            nameof(HttpStaticResponseHandler),
            sw.ElapsedMilliseconds
        );

        return response;
    }
}

public sealed record class StaticResponseHandlerOptions
{
    public bool Enabled { get; init; } = true;
    public string PathStartsWith { get; init; } = "/";
    public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;
    public string ContentType { get; init; } = "application/json";
    public string ResponseBody { get; init; } = "{}";
}
