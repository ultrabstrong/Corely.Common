using Corely.Common.Http;

namespace Corely.Common.UnitTests.Http;

public class HttpRequestResponseLoggingExtensionsTests
{
    [Fact]
    public void EnableRequestLogging_Sets_Request_Option_And_Is_Fluent()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");

        var returned = req.EnableRequestLogging();

        Assert.Same(req, returned);
        var has = req.Options.TryGetValue(
            new HttpRequestOptionsKey<bool>("LOG_REQUEST_DETAILS_OPTION"),
            out var flag
        );
        Assert.True(has);
        Assert.True(flag);
    }

    [Fact]
    public void EnableResponseLogging_Sets_Response_Option_And_Is_Fluent()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");

        var returned = req.EnableResponseLogging();

        Assert.Same(req, returned);
        var has = req.Options.TryGetValue(
            new HttpRequestOptionsKey<bool>("LOG_RESPONSE_DETAILS_OPTION"),
            out var flag
        );
        Assert.True(has);
        Assert.True(flag);
    }

    [Fact]
    public void EnableRequestResponseLogging_Sets_Both_Options()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "https://example.test/");

        req.EnableRequestResponseLogging();

        var hasReq = req.Options.TryGetValue(
            new HttpRequestOptionsKey<bool>("LOG_REQUEST_DETAILS_OPTION"),
            out var reqFlag
        );
        var hasResp = req.Options.TryGetValue(
            new HttpRequestOptionsKey<bool>("LOG_RESPONSE_DETAILS_OPTION"),
            out var respFlag
        );

        Assert.True(hasReq);
        Assert.True(reqFlag);
        Assert.True(hasResp);
        Assert.True(respFlag);
    }

    [Fact]
    public void Calling_Extensions_Multiple_Times_Is_Idempotent()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");

        req.EnableRequestLogging().EnableRequestLogging();
        req.EnableResponseLogging().EnableResponseLogging();

        var hasReq = req.Options.TryGetValue(
            new HttpRequestOptionsKey<bool>("LOG_REQUEST_DETAILS_OPTION"),
            out var reqFlag
        );
        var hasResp = req.Options.TryGetValue(
            new HttpRequestOptionsKey<bool>("LOG_RESPONSE_DETAILS_OPTION"),
            out var respFlag
        );

        Assert.True(hasReq);
        Assert.True(reqFlag);
        Assert.True(hasResp);
        Assert.True(respFlag);
    }

    [Fact]
    public void OmitRequestJsonFields_Sets_StringArray_Option()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "https://example.test/");
        req.OmitRequestJsonFields("document_url", "token");

        var has = req.Options.TryGetValue(
            new HttpRequestOptionsKey<string[]>("LOG_REQUEST_BODY_OMIT_JSON_FIELDS_OPTION"),
            out var fields
        );
        Assert.True(has);
        Assert.NotNull(fields);
        Assert.Contains("document_url", fields!);
        Assert.Contains("token", fields!);
    }

    [Fact]
    public void OmitResponseJsonFields_Sets_StringArray_Option()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "https://example.test/");
        req.OmitResponseJsonFields("secret");

        var has = req.Options.TryGetValue(
            new HttpRequestOptionsKey<string[]>("LOG_RESPONSE_BODY_OMIT_JSON_FIELDS_OPTION"),
            out var fields
        );
        Assert.True(has);
        Assert.NotNull(fields);
        Assert.Single(fields!);
        Assert.Equal("secret", fields![0]);
    }
}
