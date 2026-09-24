using Corely.Common.Extensions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Corely.Common.Http;

public sealed class HttpRequestResponseLoggingHandler(ILogger<HttpRequestResponseLoggingHandler> logger)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var overheadSw = Stopwatch.StartNew();

        if (request.ShouldLogRequestDetails())
            await LogRequestAsync(request, cancellationToken).ConfigureAwait(false);

        overheadSw.Stop();

        var sw = Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        sw.Stop();

        overheadSw.Start();

        if (request.ShouldLogResponseDetails())
            await LogResponseAsync(request, response, sw.ElapsedMilliseconds, cancellationToken)
                .ConfigureAwait(false);
        else
            logger.LogInformation(
                "HTTP {Method} {Uri} responded {StatusCode} in {ElapsedMs} ms",
                request.Method,
                request.RequestUri,
                (int)response.StatusCode,
                sw.ElapsedMilliseconds
            );

        overheadSw.Stop();
        logger.LogTrace(
            "HTTP {Handler} logging overhead {OverheadMs} ms",
            nameof(HttpRequestResponseLoggingHandler),
            overheadSw.ElapsedMilliseconds
        );

        return response;
    }

    private async Task LogRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var scopeState = new Dictionary<string, object?>
        {
            ["HttpRequestHeaders"] = request.Headers.ToLoggingSnapshot(request.Content?.Headers),
        };

        if (request.Content is not null)
        {
            try
            {
                await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);

                var body = await request
                    .Content.ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (request.TryGetRequestOmitJsonFields(out var omitFieldsReq))
                    body = body.WithJsonFieldsOmitted(omitFieldsReq);

                if (request.TryGetRequestTruncateJsonFields(out var truncReq) && truncReq != null)
                {
                    foreach (var (Field, Length) in truncReq)
                    {
                        body = body.WithJsonFieldTruncated(Field, Length);
                    }
                }

                scopeState["HttpRequestBody"] = body;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Failed to read request body for logging.");
            }
        }

        using var scope = logger.BeginScope(scopeState);
        logger.LogDebug("HTTP {Method} {Uri} request", request.Method, request.RequestUri);
    }

    private async Task LogResponseAsync(
        HttpRequestMessage request,
        HttpResponseMessage response,
        long elapsedMs,
        CancellationToken cancellationToken
    )
    {
        var scopeState = new Dictionary<string, object?>
        {
            ["HttpResponseHeaders"] = response.Headers.ToLoggingSnapshot(response.Content?.Headers),
        };

        if (response.Content is not null)
        {
            try
            {
                await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);

                var body = await response
                    .Content.ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (request.TryGetResponseOmitJsonFields(out var omitFieldsResp))
                    body = body.WithJsonFieldsOmitted(omitFieldsResp);

                if (
                    request.TryGetResponseTruncateJsonFields(out var truncResp)
                    && truncResp != null
                )
                {
                    foreach (var (Field, Length) in truncResp)
                    {
                        body = body.WithJsonFieldTruncated(Field, Length);
                    }
                }

                scopeState["HttpResponseBody"] = body;
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Failed to read response body for logging.");
            }
        }

        using var scope = logger.BeginScope(scopeState);
        logger.LogInformation(
            "HTTP {Method} {Uri} responded {StatusCode} in {ElapsedMs} ms",
            request.Method,
            request.RequestUri,
            (int)response.StatusCode,
            elapsedMs
        );
    }
}
