using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

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
            ["HttpRequestHeaders"] = BuildHeadersSnapshot(
                request.Headers,
                request.Content?.Headers
            ),
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
                    body = OmitJsonFields(body, omitFieldsReq);

                if (request.TryGetRequestTruncateJsonFields(out var truncReq) && truncReq != null)
                {
                    foreach (var (Field, Length) in truncReq)
                    {
                        body = TruncateJsonFields(body, Field, Length);
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
            ["HttpResponseHeaders"] = BuildHeadersSnapshot(
                response.Headers,
                response.Content?.Headers
            ),
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
                    body = OmitJsonFields(body, omitFieldsResp);

                if (
                    request.TryGetResponseTruncateJsonFields(out var truncResp)
                    && truncResp != null
                )
                {
                    foreach (var (Field, Length) in truncResp)
                    {
                        body = TruncateJsonFields(body, Field, Length);
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

    private string OmitJsonFields(string body, string[] fields)
    {
        try
        {
            if (fields.Length == 0)
                return body;

            var alternation = string.Join("|", fields.Select(Regex.Escape));
            var pattern = $@"""({alternation})""(\s*):(\s*)""[^""]*""";

            return Regex.Replace(
                body,
                pattern,
                m => $"\"{m.Groups[1].Value}\"{m.Groups[2].Value}:{m.Groups[3].Value}\"[OMITTED]\"",
                RegexOptions.CultureInvariant | RegexOptions.Singleline
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to omit JSON fields from body during logging.");
            return body;
        }
    }

    private string TruncateJsonFields(string body, string field, int maxLength)
    {
        try
        {
            if (string.IsNullOrEmpty(body) || string.IsNullOrWhiteSpace(field) || maxLength < 0)
                return body;

            var escapedField = Regex.Escape(field);
            var pattern = $@"""({escapedField})""(\s*):(\s*)""([^""]*)""";

            return Regex.Replace(
                body,
                pattern,
                m =>
                {
                    var value = m.Groups[4].Value;
                    var truncated = value.Length > maxLength ? value[..maxLength] : value;
                    var suffix = value.Length > maxLength ? "...[TRUNCATED]" : string.Empty;
                    return $"\"{m.Groups[1].Value}\"{m.Groups[2].Value}:{m.Groups[3].Value}\"{truncated}{suffix}\"";
                },
                RegexOptions.CultureInvariant | RegexOptions.Singleline
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to truncate JSON fields from body during logging.");
            return body;
        }
    }

    private static Dictionary<string, string> BuildHeadersSnapshot(
        HttpHeaders primary,
        HttpContentHeaders? contentHeaders
    )
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var h in primary)
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

    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Proxy-Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key",
        "Api-Key",
    };

    private static string MaskIfSensitive(string headerName, string value) =>
        SensitiveHeaders.Contains(headerName) ? "[REDACTED]" : value;
}
