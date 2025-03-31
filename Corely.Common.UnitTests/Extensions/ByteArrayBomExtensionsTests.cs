﻿using Corely.Common.Extensions;
using System.Text;

namespace Corely.Common.UnitTests.Extensions;

public class ByteArrayBomExtensionsTests
{
    [Theory, MemberData(nameof(GetByteOrderMarkEncoding))]
    public void GetByteOrderMarkEncoding_ReturnsCorrectEncoding(byte[] bom, Encoding expectedEncoding)
    {
        var encoding = bom.GetByteOrderMarkEncoding();
        Assert.Equal(expectedEncoding, encoding);
    }

    public static IEnumerable<object[]> GetByteOrderMarkEncoding() =>
    [
        [new byte[] { 0xef, 0xbb, 0xbf }, new UTF8Encoding(true)],
            [new byte[] { 0xff, 0xfe, 0x00, 0x00 }, Encoding.UTF32], //UTF-32LE
            [new byte[] { 0xff, 0xfe }, Encoding.Unicode], //UTF-16LE
            [new byte[] { 0xfe, 0xff }, Encoding.BigEndianUnicode], //UTF-16BE
            [new byte[] { 0x00, 0x00, 0xfe, 0xff }, new UTF32Encoding(true, true)],  //UTF-32BE
            [new byte[] { 0x00, 0x00, 0x00, 0x00 }, new UTF8Encoding(false)]
    ];
}
