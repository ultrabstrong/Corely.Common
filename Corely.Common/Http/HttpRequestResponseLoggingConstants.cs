namespace Corely.Common.Http;

internal static class HttpRequestResponseLoggingConstants
{
    internal const string LOG_REQUEST_DETAILS_OPTION = nameof(LOG_REQUEST_DETAILS_OPTION);
    internal const string LOG_RESPONSE_DETAILS_OPTION = nameof(LOG_RESPONSE_DETAILS_OPTION);

    internal const string LOG_REQUEST_BODY_OMIT_JSON_FIELDS_OPTION = nameof(
        LOG_REQUEST_BODY_OMIT_JSON_FIELDS_OPTION
    );
    internal const string LOG_RESPONSE_BODY_OMIT_JSON_FIELDS_OPTION = nameof(
        LOG_RESPONSE_BODY_OMIT_JSON_FIELDS_OPTION
    );

    internal const string LOG_REQUEST_BODY_TRUNCATE_JSON_FIELDS_OPTION = nameof(
        LOG_REQUEST_BODY_TRUNCATE_JSON_FIELDS_OPTION
    );
    internal const string LOG_RESPONSE_BODY_TRUNCATE_JSON_FIELDS_OPTION = nameof(
        LOG_RESPONSE_BODY_TRUNCATE_JSON_FIELDS_OPTION
    );
}
