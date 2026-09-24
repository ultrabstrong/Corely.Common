using System.Diagnostics.CodeAnalysis;

namespace Corely.Common.Http;

public static class HttpRequestResponseDetailLoggingExtensions
{
    extension(HttpRequestMessage request)
    {
        public HttpRequestMessage EnableRequestResponseDetailLogging()
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

        public HttpRequestMessage EnableRequestDetailLogging()
        {
            request.Options.Set(
                new HttpRequestOptionsKey<bool>(
                    HttpRequestResponseLoggingConstants.LOG_REQUEST_DETAILS_OPTION
                ),
                true
            );
            return request;
        }

        public HttpRequestMessage EnableResponseDetailLogging()
        {
            request.Options.Set(
                new HttpRequestOptionsKey<bool>(
                    HttpRequestResponseLoggingConstants.LOG_RESPONSE_DETAILS_OPTION
                ),
                true
            );
            return request;
        }

        public HttpRequestMessage OmitRequestJsonFields(params string[] jsonFieldNames)
        {
            request.Options.Set(
                new HttpRequestOptionsKey<string[]>(
                    HttpRequestResponseLoggingConstants.LOG_REQUEST_BODY_OMIT_JSON_FIELDS_OPTION
                ),
                jsonFieldNames
            );
            return request;
        }

        public HttpRequestMessage OmitResponseJsonFields(params string[] jsonFieldNames)
        {
            request.Options.Set(
                new HttpRequestOptionsKey<string[]>(
                    HttpRequestResponseLoggingConstants.LOG_RESPONSE_BODY_OMIT_JSON_FIELDS_OPTION
                ),
                jsonFieldNames
            );
            return request;
        }

        public HttpRequestMessage TruncateRequestJsonFields(
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

        public HttpRequestMessage TruncateResponseJsonFields(
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

        internal bool ShouldLogRequestDetails() =>
            request.Options.TryGetValue(
                new HttpRequestOptionsKey<bool>(
                    HttpRequestResponseLoggingConstants.LOG_REQUEST_DETAILS_OPTION
                ),
                out var flag
            ) && flag;

        internal bool ShouldLogResponseDetails() =>
            request.Options.TryGetValue(
                new HttpRequestOptionsKey<bool>(
                    HttpRequestResponseLoggingConstants.LOG_RESPONSE_DETAILS_OPTION
                ),
                out var flag
            ) && flag;

        internal bool TryGetRequestOmitJsonFields([NotNullWhen(true)] out string[]? fields) =>
            request.Options.TryGetValue(
                new HttpRequestOptionsKey<string[]>(
                    HttpRequestResponseLoggingConstants.LOG_REQUEST_BODY_OMIT_JSON_FIELDS_OPTION
                ),
                out fields
            );

        internal bool TryGetResponseOmitJsonFields([NotNullWhen(true)] out string[]? fields) =>
            request.Options.TryGetValue(
                new HttpRequestOptionsKey<string[]>(
                    HttpRequestResponseLoggingConstants.LOG_RESPONSE_BODY_OMIT_JSON_FIELDS_OPTION
                ),
                out fields
            );

        internal bool TryGetRequestTruncateJsonFields(
            [NotNullWhen(true)] out (string Field, int Length)[]? fields
        ) =>
            request.Options.TryGetValue(
                new HttpRequestOptionsKey<(string Field, int Length)[]>(
                    HttpRequestResponseLoggingConstants.LOG_REQUEST_BODY_TRUNCATE_JSON_FIELDS_OPTION
                ),
                out fields
            );

        internal bool TryGetResponseTruncateJsonFields(
            [NotNullWhen(true)] out (string Field, int Length)[]? fields
        ) =>
            request.Options.TryGetValue(
                new HttpRequestOptionsKey<(string Field, int Length)[]>(
                    HttpRequestResponseLoggingConstants.LOG_RESPONSE_BODY_TRUNCATE_JSON_FIELDS_OPTION
                ),
                out fields
            );
    }
}
