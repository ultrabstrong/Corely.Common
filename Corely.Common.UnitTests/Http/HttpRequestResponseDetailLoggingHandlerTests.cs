using Corely.Common.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Corely.Common.UnitTests.Http;

public class HttpRequestResponseDetailLoggingHandlerTests
{
    [Fact]
    public async Task Logs_Request_When_Enabled_IncludesHeaders_And_Body_Property()
    {
        var logger = new Mock<ILogger<HttpRequestResponseLoggingHandler>>();
        var capturedScopes = new List<object?>();
        logger
            .Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>()))
            .Callback((object state) => capturedScopes.Add(state))
            .Returns(Mock.Of<IDisposable>());

        var inner = new StubInnerHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var handler = new HttpRequestResponseLoggingHandler(logger.Object) { InnerHandler = inner };
        var client = new HttpClient(handler);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test/path?x=1")
        {
            Content = new StringContent("request-body"),
        };
        request.Headers.Add("X-Test", "abc");
        request.Headers.Add("Authorization", "Bearer secret");
        request.EnableRequestDetailLogging();

        _ = await client.SendAsync(request);

        logger.Verify(
            l =>
                l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, _) =>
                            v.ToString()!.Contains("HTTP") && v.ToString()!.Contains("request")
                    ),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );

        // Verify headers and body are in scope; Authorization must be redacted
        var scopeDict = capturedScopes
            .Select(s => s as IDictionary<string, object?>)
            .LastOrDefault(d =>
                d != null
                && (d.ContainsKey("HttpRequestHeaders") || d.ContainsKey("HttpRequestBody"))
            );
        Assert.NotNull(scopeDict);
        var headers = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(
            scopeDict!["HttpRequestHeaders"]!
        );
        Assert.Equal("[REDACTED]", headers["Authorization"]);
        Assert.Equal("request-body", scopeDict!["HttpRequestBody"]);
    }

    [Theory]
    [InlineData("Authorization")]
    [InlineData("Proxy-Authorization")]
    [InlineData("Cookie")]
    [InlineData("X-Api-Key")]
    [InlineData("Api-Key")]
    public async Task Redacts_Sensitive_Request_Headers(string headerName)
    {
        var logger = new Mock<ILogger<HttpRequestResponseLoggingHandler>>();
        var capturedScopes = new List<object?>();
        logger
            .Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>()))
            .Callback((object state) => capturedScopes.Add(state))
            .Returns(Mock.Of<IDisposable>());

        var inner = new StubInnerHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var handler = new HttpRequestResponseLoggingHandler(logger.Object) { InnerHandler = inner };
        var client = new HttpClient(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");
        request.Headers.TryAddWithoutValidation(headerName, "secret");
        request.EnableRequestDetailLogging();

        _ = await client.SendAsync(request);

        var scopeDict = capturedScopes
            .Select(s => s as IDictionary<string, object?>)
            .FirstOrDefault(d => d != null && d.ContainsKey("HttpRequestHeaders"));
        Assert.NotNull(scopeDict);
        var headers = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(
            scopeDict!["HttpRequestHeaders"]!
        );
        Assert.Equal("[REDACTED]", headers[headerName]);
    }

    [Fact]
    public async Task Logs_Response_When_Enabled_IncludesHeaders_Body_And_Latency()
    {
        var logger = new Mock<ILogger<HttpRequestResponseLoggingHandler>>();
        var capturedScopes = new List<object?>();
        logger
            .Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>()))
            .Callback((object state) => capturedScopes.Add(state))
            .Returns(Mock.Of<IDisposable>());

        var response = new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            Content = new StringContent("response-body"),
        };
        response.Headers.TryAddWithoutValidation("X-Api-Key", "secret");

        var handler = new HttpRequestResponseLoggingHandler(logger.Object)
        {
            InnerHandler = new StubInnerHandler(response),
        };
        var client = new HttpClient(handler);

        var request = new HttpRequestMessage(HttpMethod.Put, "https://example.test/res");
        request.EnableResponseDetailLogging();

        var resp = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);

        // Verify info line with latency
        logger.Verify(
            l =>
                l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, _) =>
                            v.ToString()!.Contains("responded") && v.ToString()!.Contains("ms")
                    ),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );

        // Verify headers and body in scope; sensitive header redacted
        var scopeDict = capturedScopes
            .Select(s => s as IDictionary<string, object?>)
            .LastOrDefault(d =>
                d != null
                && (d.ContainsKey("HttpResponseHeaders") || d.ContainsKey("HttpResponseBody"))
            );
        Assert.NotNull(scopeDict);
        var headers = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(
            scopeDict!["HttpResponseHeaders"]!
        );
        Assert.Equal("[REDACTED]", headers["X-Api-Key"]);
        Assert.Equal("response-body", scopeDict!["HttpResponseBody"]);

        // Response content should remain readable after logging
        var bodyAgain = await resp.Content!.ReadAsStringAsync();
        Assert.Equal("response-body", bodyAgain);
    }

    [Fact]
    public async Task Logs_Basic_Info_When_Response_Logging_Disabled()
    {
        var logger = new Mock<ILogger<HttpRequestResponseLoggingHandler>>();

        var handler = new HttpRequestResponseLoggingHandler(logger.Object)
        {
            InnerHandler = new StubInnerHandler(new HttpResponseMessage(HttpStatusCode.OK)),
        };
        var client = new HttpClient(handler);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");
        // Do not set LogResponseKey

        _ = await client.SendAsync(request);

        logger.Verify(
            l =>
                l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, _) =>
                            v.ToString()!.Contains("responded") && v.ToString()!.Contains("ms")
                    ),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task Omits_Configured_Json_Fields_From_Request_And_Response_Bodies()
    {
        var logger = new Mock<ILogger<HttpRequestResponseLoggingHandler>>();
        var capturedScopes = new List<object?>();
        logger
            .Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>()))
            .Callback((object state) => capturedScopes.Add(state))
            .Returns(Mock.Of<IDisposable>());

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"document_url\":\"abc\",\"ok\":true}"),
        };

        var handler = new HttpRequestResponseLoggingHandler(logger.Object)
        {
            InnerHandler = new StubInnerHandler(response),
        };
        var client = new HttpClient(handler);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test/")
        {
            Content = new StringContent("{\"document_url\":\"xyz\",\"name\":\"n\"}"),
        };
        request.EnableRequestDetailLogging();
        request.EnableResponseDetailLogging();
        request.OmitRequestJsonFields("document_url");
        request.OmitResponseJsonFields("document_url");

        _ = await client.SendAsync(request);

        var reqScope = capturedScopes
            .Select(s => s as IDictionary<string, object?>)
            .FirstOrDefault(d => d != null && d.ContainsKey("HttpRequestBody"));
        Assert.NotNull(reqScope);
        Assert.Equal(
            "{\"document_url\":\"[OMITTED]\",\"name\":\"n\"}",
            reqScope!["HttpRequestBody"]
        );

        var respScope = capturedScopes
            .Select(s => s as IDictionary<string, object?>)
            .FirstOrDefault(d => d != null && d.ContainsKey("HttpResponseBody"));
        Assert.NotNull(respScope);
        Assert.Equal(
            "{\"document_url\":\"[OMITTED]\",\"ok\":true}",
            respScope!["HttpResponseBody"]
        );
    }

    [Fact]
    public async Task Omits_Preserving_Whitespace_Around_Colon()
    {
        var logger = new Mock<ILogger<HttpRequestResponseLoggingHandler>>();
        var capturedScopes = new List<object?>();
        logger
            .Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>()))
            .Callback((object state) => capturedScopes.Add(state))
            .Returns(Mock.Of<IDisposable>());

        var inner = new StubInnerHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var handler = new HttpRequestResponseLoggingHandler(logger.Object) { InnerHandler = inner };
        var client = new HttpClient(handler);

        var json = "{\"document_url\"  :   \"xyz\" , \"name\" : \"n\"}";
        var expected = "{\"document_url\"  :   \"[OMITTED]\" , \"name\" : \"n\"}";
        var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test/")
        {
            Content = new StringContent(json),
        };
        request.EnableRequestDetailLogging();
        request.OmitRequestJsonFields("document_url");

        _ = await client.SendAsync(request);

        var reqScope = capturedScopes
            .Select(s => s as IDictionary<string, object?>)
            .FirstOrDefault(d => d != null && d.ContainsKey("HttpRequestBody"));
        Assert.NotNull(reqScope);
        Assert.Equal(expected, reqScope!["HttpRequestBody"]);
    }

    private sealed class StubInnerHandler(HttpResponseMessage toReturn) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromResult(toReturn);
    }
}
