using System.Diagnostics.CodeAnalysis;

namespace Corely.Common.Http;

public static class HttpRequestResponseDetailLoggingExtensions
{
    public static HttpRequestMessage EnableRequestResponseDetailLogging(this HttpRequestMessage request)
    {
        request.Options.Set(
            new HttpRequestOptionsKey<bool>(
                HttpRequestResponseLoggingConstants.LOG_REQUEST_DETAILS_OPTION
            ),
            true
        );
        request.Options.Set(
            new HttpRequestOptionsKey<bool>(
                HttpRequestResponseLoggingConstants.LOG_RESPONSE_DETAILS_OPTION
            ),
            true
        );
        return request;
    }

    public static HttpRequestMessage EnableRequestDetailLogging(this HttpRequestMessage request)
    {
        request.Options.Set(
            new HttpRequestOptionsKey<bool>(
                HttpRequestResponseLoggingConstants.LOG_REQUEST_DETAILS_OPTION
            ),
            true
        );
        return request;
    }

    public static HttpRequestMessage EnableResponseDetailLogging(this HttpRequestMessage request)
    {
        request.Options.Set(
            new HttpRequestOptionsKey<bool>(
                HttpRequestResponseLoggingConstants.LOG_RESPONSE_DETAILS_OPTION
            ),
            true
        );
        return request;
    }

    public static HttpRequestMessage OmitRequestJsonFields(
        this HttpRequestMessage request,
        params string[] jsonFieldNames
    )
    {
        request.Options.Set(
            new HttpRequestOptionsKey<string[]>(
                HttpRequestResponseLoggingConstants.LOG_REQUEST_BODY_OMIT_JSON_FIELDS_OPTION
            ),
            jsonFieldNames
        );
        return request;
    }

    public static HttpRequestMessage OmitResponseJsonFields(
        this HttpRequestMessage request,
        params string[] jsonFieldNames
    )
    {
        request.Options.Set(
            new HttpRequestOptionsKey<string[]>(
                HttpRequestResponseLoggingConstants.LOG_RESPONSE_BODY_OMIT_JSON_FIELDS_OPTION
            ),
            jsonFieldNames
        );
        return request;
    }

    public static HttpRequestMessage TruncateRequestJsonFields(
        this HttpRequestMessage request,
        params (string Field, int Length)[] fields
    )
    {
        request.Options.Set(
            new HttpRequestOptionsKey<(string Field, int Length)[]>(
                HttpRequestResponseLoggingConstants.LOG_REQUEST_BODY_TRUNCATE_JSON_FIELDS_OPTION
            ),
            fields
        );
        return request;
    }

    public static HttpRequestMessage TruncateResponseJsonFields(
        this HttpRequestMessage request,
        params (string Field, int Length)[] fields
    )
    {
        request.Options.Set(
            new HttpRequestOptionsKey<(string Field, int Length)[]>(
                HttpRequestResponseLoggingConstants.LOG_RESPONSE_BODY_TRUNCATE_JSON_FIELDS_OPTION
            ),
            fields
        );
        return request;
    }

    internal static bool ShouldLogRequestDetails(this HttpRequestMessage request) =>
        request.Options.TryGetValue(
            new HttpRequestOptionsKey<bool>(
                HttpRequestResponseLoggingConstants.LOG_REQUEST_DETAILS_OPTION
            ),
            out var flag
        ) && flag;

    internal static bool ShouldLogResponseDetails(this HttpRequestMessage request) =>
        request.Options.TryGetValue(
            new HttpRequestOptionsKey<bool>(
                HttpRequestResponseLoggingConstants.LOG_RESPONSE_DETAILS_OPTION
            ),
            out var flag
        ) && flag;

    internal static bool TryGetRequestOmitJsonFields(
        this HttpRequestMessage request,
        [NotNullWhen(true)] out string[]? fields
    ) =>
        request.Options.TryGetValue(
            new HttpRequestOptionsKey<string[]>(
                HttpRequestResponseLoggingConstants.LOG_REQUEST_BODY_OMIT_JSON_FIELDS_OPTION
            ),
            out fields
        );

    internal static bool TryGetResponseOmitJsonFields(
        this HttpRequestMessage request,
        [NotNullWhen(true)] out string[]? fields
    ) =>
        request.Options.TryGetValue(
            new HttpRequestOptionsKey<string[]>(
                HttpRequestResponseLoggingConstants.LOG_RESPONSE_BODY_OMIT_JSON_FIELDS_OPTION
            ),
            out fields
        );

    internal static bool TryGetRequestTruncateJsonFields(
        this HttpRequestMessage request,
        [NotNullWhen(true)] out (string Field, int Length)[]? fields
    ) =>
        request.Options.TryGetValue(
            new HttpRequestOptionsKey<(string Field, int Length)[]>(
                HttpRequestResponseLoggingConstants.LOG_REQUEST_BODY_TRUNCATE_JSON_FIELDS_OPTION
            ),
            out fields
        );

    internal static bool TryGetResponseTruncateJsonFields(
        this HttpRequestMessage request,
        [NotNullWhen(true)] out (string Field, int Length)[]? fields
    ) =>
        request.Options.TryGetValue(
            new HttpRequestOptionsKey<(string Field, int Length)[]>(
                HttpRequestResponseLoggingConstants.LOG_RESPONSE_BODY_TRUNCATE_JSON_FIELDS_OPTION
            ),
            out fields
        );
}
