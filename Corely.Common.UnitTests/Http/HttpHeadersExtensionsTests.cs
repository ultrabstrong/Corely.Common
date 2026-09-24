using Corely.Common.Http;

namespace Corely.Common.UnitTests.Http;

public class HttpHeadersExtensionsTests
{
    [Theory]
    [InlineData("Authorization")]
    [InlineData("proxy-authorization")]
    [InlineData("Cookie")]
    [InlineData("X-Api-Key")]
    [InlineData("Api-Key")]
    public void ToLoggingSnapshot_RedactsSensitiveHeaders(string name)
    {
        var request = new HttpRequestMessage();
        request.Headers.TryAddWithoutValidation(name, "secret");

        var snapshot = request.Headers.ToLoggingSnapshot(null);

        Assert.Equal("[REDACTED]", snapshot[name]);
    }

    [Fact]
    public void ToLoggingSnapshot_JoinsMultipleValues()
    {
        var request = new HttpRequestMessage();
        request.Headers.Add("X-Trace", ["a", "b"]);

        var snapshot = request.Headers.ToLoggingSnapshot(null);

        Assert.Equal("a,b", snapshot["X-Trace"]);
    }

    [Fact]
    public void ToLoggingSnapshot_PrefixesContentHeaders()
    {
        var request = new HttpRequestMessage { Content = new StringContent("x") };

        var snapshot = request.Headers.ToLoggingSnapshot(request.Content.Headers);

        Assert.Equal("text/plain; charset=utf-8", snapshot["Content-Content-Type"]);
    }
}
