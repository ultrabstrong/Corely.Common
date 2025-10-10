using Corely.Common.Http;
using Microsoft.Extensions.Logging;

namespace Corely.Common.UnitTests.Http;

public class HttpErrorLoggingExtensionsTests
{
    [Fact]
    public void SetErrorLogLevel_Sets_Option_And_Is_Fluent()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");

        var returned = req.SetErrorLogLevel(LogLevel.Warning);

        Assert.Same(req, returned);
        var has = req.Options.TryGetValue(
            new HttpRequestOptionsKey<LogLevel>("ERROR_LOG_LEVEL_OPTION"),
            out var level
        );
        Assert.True(has);
        Assert.Equal(LogLevel.Warning, level);
    }

    [Fact]
    public void SetErrorLogLevel_Is_Overridable()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "https://example.test/");

        req.SetErrorLogLevel(LogLevel.Information);
        req.SetErrorLogLevel(LogLevel.Critical);

        var has = req.Options.TryGetValue(
            new HttpRequestOptionsKey<LogLevel>("ERROR_LOG_LEVEL_OPTION"),
            out var level
        );
        Assert.True(has);
        Assert.Equal(LogLevel.Critical, level);
    }

    [Fact]
    public void ErrorLogLevel_Not_Set_Option_Is_Absent()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");

        var has = req.Options.TryGetValue(
            new HttpRequestOptionsKey<LogLevel>("ERROR_LOG_LEVEL_OPTION"),
            out var level
        );
        Assert.False(has);
    }
}
