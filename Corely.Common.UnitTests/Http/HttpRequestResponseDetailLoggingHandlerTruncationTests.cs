using Corely.Common.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Corely.Common.UnitTests.Http;

public class HttpRequestResponseDetailLoggingHandlerTruncationTests
{
    [Fact]
    public async Task Truncates_Configured_Json_Fields_In_Request_And_Response_Bodies()
    {
        var logger = new Mock<ILogger<HttpRequestResponseLoggingHandler>>();
        var capturedScopes = new List<object?>();
        logger
            .Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>()))
            .Callback((object state) => capturedScopes.Add(state))
            .Returns(Mock.Of<IDisposable>());

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"document_url\":\"abcdef\",\"ok\":true}"),
        };

        var handler = new HttpRequestResponseLoggingHandler(logger.Object)
        {
            InnerHandler = new StubInnerHandler(response),
        };
        var client = new HttpClient(handler);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test/")
        {
            Content = new StringContent("{\"document_url\":\"xyz123\",\"name\":\"n\"}"),
        };
        request.EnableRequestDetailLogging();
        request.EnableResponseDetailLogging();
        request.TruncateRequestJsonFields(("document_url", 3));
        request.TruncateResponseJsonFields(("document_url", 3));

        _ = await client.SendAsync(request);

        var reqScope = capturedScopes
            .Select(s => s as IDictionary<string, object?>)
            .FirstOrDefault(d => d != null && d.ContainsKey("HttpRequestBody"));
        Assert.NotNull(reqScope);
        Assert.Equal(
            "{\"document_url\":\"xyz...[TRUNCATED]\",\"name\":\"n\"}",
            reqScope!["HttpRequestBody"]
        );

        var respScope = capturedScopes
            .Select(s => s as IDictionary<string, object?>)
            .FirstOrDefault(d => d != null && d.ContainsKey("HttpResponseBody"));
        Assert.NotNull(respScope);
        Assert.Equal(
            "{\"document_url\":\"abc...[TRUNCATED]\",\"ok\":true}",
            respScope!["HttpResponseBody"]
        );
    }

    [Fact]
    public async Task Does_Not_Append_Truncated_Suffix_When_Value_Shorter_Than_Max()
    {
        var logger = new Mock<ILogger<HttpRequestResponseLoggingHandler>>();
        var capturedScopes = new List<object?>();
        logger
            .Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>()))
            .Callback((object state) => capturedScopes.Add(state))
            .Returns(Mock.Of<IDisposable>());

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"name\":\"bob\"}"),
        };

        var handler = new HttpRequestResponseLoggingHandler(logger.Object)
        {
            InnerHandler = new StubInnerHandler(response),
        };
        var client = new HttpClient(handler);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test/")
        {
            Content = new StringContent("{\"name\":\"ann\"}"),
        };
        request.EnableRequestDetailLogging();
        request.EnableResponseDetailLogging();
        request.TruncateRequestJsonFields(("name", 10));
        request.TruncateResponseJsonFields(("name", 10));

        _ = await client.SendAsync(request);

        var reqScope = capturedScopes
            .Select(s => s as IDictionary<string, object?>)
            .FirstOrDefault(d => d != null && d.ContainsKey("HttpRequestBody"));
        Assert.NotNull(reqScope);
        Assert.Equal("{\"name\":\"ann\"}", reqScope!["HttpRequestBody"]);

        var respScope = capturedScopes
            .Select(s => s as IDictionary<string, object?>)
            .FirstOrDefault(d => d != null && d.ContainsKey("HttpResponseBody"));
        Assert.NotNull(respScope);
        Assert.Equal("{\"name\":\"bob\"}", respScope!["HttpResponseBody"]);
    }

    private sealed class StubInnerHandler(HttpResponseMessage toReturn) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromResult(toReturn);
    }
}
