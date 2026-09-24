using Corely.Common.File;

namespace Corely.Common.UnitTests.File;

public class FileInfoExtensionsTests
{
    [Theory]
    [InlineData("report.txt", "report")]
    [InlineData("archive.tar.gz", "archive.tar")]
    [InlineData("noextension", "noextension")]
    [InlineData("txt.txt", "txt")]
    public void NameWithoutExtension_RemovesOnlyTheLastExtension(string name, string expected)
    {
        Assert.Equal(expected, new FileInfo(name).NameWithoutExtension());
    }

    [Theory]
    [InlineData("report.txt", 2, "report-[2].txt")]
    [InlineData("archive.tar.gz", 1, "archive.tar-[1].gz")]
    [InlineData("noextension", 3, "noextension-[3]")]
    public void NumberedName_PutsTheNumberBeforeTheExtension(
        string name,
        int number,
        string expected
    )
    {
        Assert.Equal(expected, new FileInfo(name).NumberedName(number));
    }
}
