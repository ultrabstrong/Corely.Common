using Corely.Common.Http;

namespace Corely.Common.UnitTests.Http;

public class HttpRequestResponseDetailLoggingTruncationExtensionsTests
{
    [Fact]
    public void TruncateRequestJsonFields_Sets_TupleArray_Option()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "https://example.test/");
        req.TruncateRequestJsonFields(("document_url", 5), ("token", 3));

        var has = req.Options.TryGetValue(
            new HttpRequestOptionsKey<(string Field, int Length)[]>(
                "LOG_REQUEST_BODY_TRUNCATE_JSON_FIELDS_OPTION"
            ),
            out var fields
        );
        Assert.True(has);
        Assert.NotNull(fields);
        Assert.Contains(fields!, f => f.Field == "document_url" && f.Length == 5);
        Assert.Contains(fields!, f => f.Field == "token" && f.Length == 3);
    }

    [Fact]
    public void TruncateResponseJsonFields_Sets_TupleArray_Option()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "https://example.test/");
        req.TruncateResponseJsonFields(("secret", 2));

        var has = req.Options.TryGetValue(
            new HttpRequestOptionsKey<(string Field, int Length)[]>(
                "LOG_RESPONSE_BODY_TRUNCATE_JSON_FIELDS_OPTION"
            ),
            out var fields
        );
        Assert.True(has);
        Assert.NotNull(fields);
        Assert.Single(fields!);
        Assert.Equal("secret", fields![0].Field);
        Assert.Equal(2, fields![0].Length);
    }
}
