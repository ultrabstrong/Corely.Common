using Corely.Common.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Corely.Common.UnitTests.Http;

public class HttpStaticResponseHandlerTests
{
    [Fact]
    public async Task Returns_Static_Response_When_Enabled_And_Path_Matches()
    {
        var logger = new Mock<ILogger<HttpStaticResponseHandler>>();
        var options = new StaticResponseHandlerOptions
        {
            Enabled = true,
            PathStartsWith = "/test/endpoint",
            StatusCode = HttpStatusCode.Accepted,
            ResponseBody = "{\"ok\":true,\"value\":123}",
            ContentType = "application/json",
        };

        var handler = new HttpStaticResponseHandler(logger.Object, options)
        {
            InnerHandler = new ShouldNotBeCalledHandler(),
        };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        var resp = await client.GetAsync("/test/endpoint?id=9");

        Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Equal("{\"ok\":true,\"value\":123}", json);

        logger.Verify(
            l =>
                l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v!.ToString()!.Contains("intercepted")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Passes_Through_When_Disabled()
    {
        var logger = new Mock<ILogger<HttpStaticResponseHandler>>();
        var options = new StaticResponseHandlerOptions
        {
            Enabled = false,
            PathStartsWith = "/test",
        };

        var innerResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("inner"),
        };
        var handler = new HttpStaticResponseHandler(logger.Object, options)
        {
            InnerHandler = new StubInnerHandler(innerResponse),
        };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        var resp = await client.GetAsync("/test/anything");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Equal("inner", body);

        logger.Verify(
            l =>
                l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v!.ToString()!.Contains("intercepted")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Passes_Through_When_Path_Does_Not_Match()
    {
        var logger = new Mock<ILogger<HttpStaticResponseHandler>>();
        var options = new StaticResponseHandlerOptions
        {
            Enabled = true,
            PathStartsWith = "/exact",
        };

        var innerResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("inner2"),
        };
        var handler = new HttpStaticResponseHandler(logger.Object, options)
        {
            InnerHandler = new StubInnerHandler(innerResponse),
        };
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };

        var resp = await client.GetAsync("/different");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Equal("inner2", body);

        logger.Verify(
            l =>
                l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v!.ToString()!.Contains("intercepted")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    private sealed class StubInnerHandler(HttpResponseMessage toReturn) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromResult(toReturn);
    }

    private sealed class ShouldNotBeCalledHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) =>
            throw new InvalidOperationException(
                "Inner handler should not be called when stub intercepts"
            );
    }
}
