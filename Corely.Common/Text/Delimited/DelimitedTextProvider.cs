using Corely.Common.Extensions;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Corely.Common.Text.Delimited;

public sealed class DelimitedTextProvider : IDelimitedTextProvider
{
    private readonly ILogger<DelimitedTextProvider> _logger;
    private readonly char _tokenDelimiter;
    private readonly char _tokenLiteral;
    private readonly string _recordDelimiter;

    public DelimitedTextProvider(ILogger<DelimitedTextProvider> logger)
        : this(logger, ',', '"', Environment.NewLine)
    {
    }

    public DelimitedTextProvider(
        ILogger<DelimitedTextProvider> logger,
        TokenDelimiter delimiter)
    {
        _logger = logger.ThrowIfNull(nameof(logger));

        (_tokenDelimiter, _tokenLiteral, _recordDelimiter) =
            delimiter switch
            {
                TokenDelimiter.Semicolon => (';', '"', Environment.NewLine),
                TokenDelimiter.Pipe => ('|', '"', Environment.NewLine),
                TokenDelimiter.Tab => ('\t', '"', Environment.NewLine),
                _ => (',', '"', Environment.NewLine),
            };
    }

    public DelimitedTextProvider(
        ILogger<DelimitedTextProvider> logger,
        char tokenDelimiter,
        char tokenLiteral,
        string recordDelimiter)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(recordDelimiter, nameof(recordDelimiter));
        (_logger, _tokenDelimiter, _tokenLiteral, _recordDelimiter) = (logger, tokenDelimiter, tokenLiteral, recordDelimiter);
    }

    public List<ReadRecordResult> ReadAllRecords(Stream stream)
    {
        _logger.LogInformation("Reading all records from stream");
        List<ReadRecordResult> records = [];
        ReadRecordResult record = new();
        do
        {
            record = ReadNextRecord(stream, record.EndPosition);
            records.Add(record);
        }
        while (record.HasMore);

        _logger.LogInformation("Finished reading {RecordCount} records from stream", records.Count);
        return records;
    }

    public ReadRecordResult ReadNextRecord(Stream stream, long startPosition)
    {
        _logger.LogDebug("Reading next record from stream");
        ReadRecordResult result;

        byte[] bom = new byte[4];
        stream.Read(bom, 0, 4);
        Encoding encoding = bom.GetByteOrderMarkEncoding();
        stream.Position = startPosition;

        long streamLength = stream.Length;
        result = ReadNextRecord(stream, encoding);
        result.StartPosition = startPosition;

        if (result != null && result.EndPosition >= streamLength)
        {
            result.HasMore = false;
        }
        _logger.LogDebug("Finished reading next record from stream");
        return result ?? new() { HasMore = false };
    }

    private ReadRecordResult ReadNextRecord(Stream stream, Encoding encoding)
    {
        ReadRecordResult result = new();

        StreamReader streamReader = new(stream, encoding);
        if (stream.Position == 0 && Equals(new UTF8Encoding(true), streamReader.CurrentEncoding)) { result.Length += 3; }

        string currentToken = string.Empty;
        int currentRecordDelim = 0;

        bool isInLiteral = false,
            lastCharEscaped = false,
            lastTokenLiteralEscaped = false;

        while (!streamReader.EndOfStream)
        {
            char c = (char)streamReader.Read();
            result.Length += streamReader.CurrentEncoding.GetByteCount([c]);
            if (c == _tokenLiteral)
            {
                if (!isInLiteral)
                {
                    if (string.IsNullOrEmpty(currentToken))
                    {
                        isInLiteral = true;
                    }
                    else if (lastTokenLiteralEscaped
                        && currentToken[^1] == _tokenLiteral)
                    {
                        isInLiteral = true;
                        lastCharEscaped = true;
                        lastTokenLiteralEscaped = true;
                    }
                    else if (currentToken.Length > 0
                        && currentToken[^1] != _tokenLiteral)
                    {
                        currentToken += c;
                        lastTokenLiteralEscaped = false;
                    }
                }
                else
                {
                    if (currentToken.Length > 0
                        && currentToken[^1] == _tokenLiteral)
                    {
                        if (lastCharEscaped)
                        {
                            currentToken += c;
                            lastCharEscaped = false;
                            lastTokenLiteralEscaped = false;
                        }
                        else
                        {
                            lastCharEscaped = true;
                            lastTokenLiteralEscaped = true;
                        }
                    }
                    else if (currentToken.Length == 0)
                    {
                        currentToken += c;
                        isInLiteral = false;
                        lastTokenLiteralEscaped = true;
                    }
                    else
                    {
                        currentToken += c;
                        lastCharEscaped = false;
                        lastTokenLiteralEscaped = false;
                    }
                }
            }
            else if (c == _tokenDelimiter)
            {
                if (isInLiteral)
                {
                    if (currentToken.Length > 0
                        && currentToken[^1] == _tokenLiteral
                        && !lastCharEscaped)
                    {
                        currentToken = currentToken[..^1];
                        result.Tokens.Add(currentToken);
                        currentToken = string.Empty;
                        isInLiteral = false;
                    }
                    else
                    {
                        currentToken += c;
                    }
                }
                else
                {
                    result.Tokens.Add(currentToken);
                    currentToken = string.Empty;
                    isInLiteral = false;
                }
                lastCharEscaped = false;
            }
            else
            {
                currentToken += c;
                lastCharEscaped = false;
            }

            if (c == _recordDelimiter[currentRecordDelim])
            {
                if (!isInLiteral)
                {
                    if (currentRecordDelim == _recordDelimiter.Length - 1)
                    {
                        currentToken = currentToken[..^_recordDelimiter.Length];
                        break;
                    }
                    else
                    {
                        currentRecordDelim++;
                    }
                }
                else
                {
                    if (currentRecordDelim == _recordDelimiter.Length - 1)
                    {
                        if (currentToken.Length != _recordDelimiter.Length
                            && currentToken[currentToken.Length - _recordDelimiter.Length - 1] == _tokenLiteral)
                        {
                            if (lastTokenLiteralEscaped)
                            {
                                currentRecordDelim = 0;
                            }
                            else
                            {
                                currentToken = currentToken[..(currentToken.Length - _recordDelimiter.Length - 1)];
                                isInLiteral = false;
                                break;
                            }
                        }
                        else
                        {
                            currentRecordDelim = 0;
                        }
                    }
                    else
                    {
                        currentRecordDelim++;
                    }
                }
            }
            else
            {
                currentRecordDelim = 0;
            }
        }
        if (isInLiteral
            && currentToken.Length > 0
            && currentToken[^1] == _tokenLiteral
            && !lastCharEscaped)
        {
            currentToken = currentToken[..^1];
        }
        result.Tokens.Add(currentToken);
        return result;
    }

    public void WriteAllRecords(IEnumerable<IEnumerable<string>> records, Stream writeTo)
    {
        _logger.LogInformation("Writing all records to stream");
        List<IEnumerable<string>> recordsList = [.. records];

        StreamWriter writer = new(writeTo, Encoding.UTF8);
        if (recordsList.Count > 0)
        {
            WriteRecord(recordsList[0], writer);
        }

        for (int i = 1; i < recordsList.Count; i++)
        {
            writer.Write(_recordDelimiter);
            WriteRecord(recordsList[i], writer);
        }
        writer.Flush();
        _logger.LogInformation("Finished writing all records to stream");
    }

    public void WriteRecord(IEnumerable<string> record, Stream writeTo)
    {
        _logger.LogDebug("Writing record to stream");
        StreamWriter writer = new(writeTo, Encoding.UTF8);
        WriteRecord(record, writer);
        writer.Write(_recordDelimiter);
        writer.Flush();
        _logger.LogDebug("Finished writing record to stream");
    }

    private void WriteRecord(IEnumerable<string> record, StreamWriter writer)
    {
        List<string> recordList = [.. record];

        if (recordList.Count > 0)
        {
            AppendTokenLiteral(recordList[0], writer);
        }

        for (int i = 1; i < recordList.Count; i++)
        {
            writer.Write(_tokenDelimiter);
            AppendTokenLiteral(recordList[i], writer);
        }
    }

    private void AppendTokenLiteral(string token, StreamWriter writer)
    {
        token ??= string.Empty;
        token = token.Replace(_tokenLiteral.ToString(), $"{_tokenLiteral}{_tokenLiteral}");

        if (token.Contains(_tokenDelimiter) || token.Contains(_recordDelimiter))
        {
            token = $"{_tokenLiteral}{token}{_tokenLiteral}";
        }
        writer.Write(token);
    }
}
