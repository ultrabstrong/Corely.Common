using Corely.Common.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Corely.Common.UnitTests.Http;

public class HttpErrorLoggingHandlerTests
{
    [Fact]
    public async Task Logs_On_NonSuccess_Status()
    {
        var logger = new Mock<ILogger<HttpErrorLoggingHandler>>();

        var innerHandler = new StubInnerHandler(
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("sensitive details"),
            }
        );

        var handler = new HttpErrorLoggingHandler(logger.Object) { InnerHandler = innerHandler };
        var client = new HttpClient(handler);

        var response = await client.GetAsync("https://example.com/");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        logger.Verify(
            l =>
                l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("HTTP request failed")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    private sealed class StubInnerHandler(HttpResponseMessage toReturn) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromResult(toReturn);
    }
}
