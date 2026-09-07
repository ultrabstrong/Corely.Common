using Corely.Common.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Corely.Common.UnitTests.Http;

public class HttpResponseMessageJsonExtensionsTests
{
    private sealed record SampleDto(string Name, int Value);

    private static HttpResponseMessage CreateJsonResponse(
        string json,
        string? contentType = "application/json"
    )
    {
        var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, contentType!),
        };
        return resp;
    }

    [Fact]
    public async Task ReadJsonBodyAsync_Returns_Object_On_Valid_Json()
    {
        var json = "{\"Name\":\"alpha\",\"Value\":42}";
        using var resp = CreateJsonResponse(json);

        var result = await resp.ReadJsonBodyAsync<SampleDto>();

        Assert.NotNull(result);
        Assert.Equal("alpha", result!.Name);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task ReadJsonBodyAsync_Is_CaseInsensitive()
    {
        // Deliberately different casing
        var json = "{\"name\":\"beta\",\"value\":7}";
        using var resp = CreateJsonResponse(json);

        var result = await resp.ReadJsonBodyAsync<SampleDto>();

        Assert.NotNull(result);
        Assert.Equal("beta", result!.Name);
        Assert.Equal(7, result.Value);
    }

    [Fact]
    public async Task ReadJsonBodyAsync_Returns_Null_And_Logs_On_Invalid_Json()
    {
        var invalidJson = "{\"Name\":\"gamma\",\"Value\":not-a-number}"; // invalid number
        using var resp = CreateJsonResponse(invalidJson);
        var logger = new TestLogger();

        var result = await resp.ReadJsonBodyAsync<SampleDto>(logger: logger);

        Assert.Null(result);
        Assert.Contains(
            logger.Entries,
            e =>
                e.level == LogLevel.Warning
                && e.exception is not null
                && e.message.Contains("Failed to deserialize")
                && e.message.Contains(nameof(SampleDto))
        );
    }

    [Fact]
    public async Task ReadJsonBodyAsync_Returns_Null_When_Response_Is_Null()
    {
        HttpResponseMessage? resp = null;
        var result = await resp.ReadJsonBodyAsync<SampleDto>();
        Assert.Null(result);
    }

    [Fact]
    public async Task ReadJsonBodyAsync_Returns_Null_When_Content_Is_Null()
    {
        using var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = null };
        var result = await resp.ReadJsonBodyAsync<SampleDto>();
        Assert.Null(result);
    }

    [Fact]
    public async Task ReadJsonBodyAsync_Deserializes_From_A_NonSeekable_Stream()
    {
        // The other tests use StringContent, which is already in memory and so would pass whether
        // or not the content were buffered first. A real network response is forward-only, which
        // is what this covers.
        var json = "{\"Name\":\"delta\",\"Value\":13}";
        using var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StreamContent(
                new ForwardOnlyStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            ),
        };

        var result = await resp.ReadJsonBodyAsync<SampleDto>();

        Assert.NotNull(result);
        Assert.Equal("delta", result!.Name);
        Assert.Equal(13, result.Value);
    }

    /// <summary>
    /// Read-once, non-seekable, and unaware of its own length - the shape of a chunked network
    /// response.
    /// </summary>
    private sealed class ForwardOnlyStream(Stream inner) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            inner.Read(buffer, offset, count);

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken
        ) => inner.ReadAsync(buffer, offset, count, cancellationToken);

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
        ) => inner.ReadAsync(buffer, cancellationToken);

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                inner.Dispose();
            base.Dispose(disposing);
        }
    }

    private sealed class TestLogger : ILogger
    {
        public List<(LogLevel level, Exception? exception, string message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            Entries.Add((logLevel, exception, formatter(state, exception)));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose() { }
        }
    }
}
