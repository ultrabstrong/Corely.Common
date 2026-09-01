﻿﻿using Corely.Common.File;
using Corely.Common.UnitTests.ClassData;

namespace Corely.Common.UnitTests.File;

public class TestableFilePathProvider : FilePathProvider
{
    public override bool DoesFileExist(string filepath)
    {
        return base.DoesFileExist(filepath);
    }
}

public class FilePathProviderTests
{
    // System.IO.Path is platform-dependent: a backslash separates directories on Windows and is an
    // ordinary filename character everywhere else. Hard-coded Windows paths therefore assert
    // nothing meaningful when the suite runs on Linux, so paths are built for the host instead.
    private static readonly string Root = OperatingSystem.IsWindows() ? @"C:\" : "/";

    private static string P(params string[] segments) =>
        System.IO.Path.Combine([Root, .. segments]);

    private readonly Mock<TestableFilePathProvider> _filePathProviderMock = new() { CallBase = true };
    private bool _doesFileExist;

    private void SetupStandardReturnForDoesFileExist()
    {
        _filePathProviderMock
            .Setup(m => m.DoesFileExist(It.IsAny<string>()))
            .Returns(() => _doesFileExist);
    }


    [Theory, ClassData(typeof(NullEmptyAndWhitespace))]
    public void DoesFileExist_WhenPathIsNullOrWhitespace_ReturnsFalse(string path)
    {
        _doesFileExist = false;
        SetupStandardReturnForDoesFileExist();

        Assert.False(_filePathProviderMock.Object.DoesFileExist(path));
    }

    [Fact]
    public void DoesFileExist_WhenFileDoesNotExist_ReturnsFalse()
    {
        _doesFileExist = false;
        SetupStandardReturnForDoesFileExist();

        Assert.False(
            _filePathProviderMock.Object.DoesFileExist(P("file_that_does_not_exist.txt"))
        );
    }

    [Fact]
    public void DoesFileExist_WhenFileExists_ReturnsTrue()
    {
        _doesFileExist = true;
        SetupStandardReturnForDoesFileExist();

        Assert.True(_filePathProviderMock.Object.DoesFileExist(P("file_that_exists.txt")));
    }

    [Theory]
    [MemberData(nameof(GetOverwriteProtectedPathTestData), 3)]
    [MemberData(nameof(GetOverwriteProtectedPathTestData), 1)]
    [MemberData(nameof(GetOverwriteProtectedPathTestData), 0)]
    public void GetOverwriteProtectedPath_WhenFileExists_ReturnsOverwriteProtectedPath(int number, string path, string expected)
    {
        SetupDoesFileExistForGetOverwriteProtectedPath(number);

        Assert.Equal(expected, _filePathProviderMock.Object.GetOverwriteProtectedPath(path));
    }

    public static IEnumerable<object[]> GetOverwriteProtectedPathTestData(int number)
    {
        static string append(int i) => i < 1 ? string.Empty : $"-[{i}]";

        yield return [number, P("file_that_exists.txt"), P($"file_that_exists{append(number)}.txt")];
        yield return [number, P("config.json.sample"), P($"config.json{append(number)}.sample")];
        yield return [number, P("config"), P($"config{append(number)}")];
        yield return [number, P("test.txt.txt"), P($"test.txt{append(number)}.txt")];
        yield return [number, P("nest1", "nest2", "file_that_exists.txt"), P("nest1", "nest2", $"file_that_exists{append(number)}.txt")];
        yield return [number, P("nest1", "nest2", "config.json.sample"), P("nest1", "nest2", $"config.json{append(number)}.sample")];
        yield return [number, P("nest1", "nest2", "config"), P("nest1", "nest2", $"config{append(number)}")];
        yield return [number, P("nest1", "nest2", "test.txt.txt"), P("nest1", "nest2", $"test.txt{append(number)}.txt")];
    }

    private void SetupDoesFileExistForGetOverwriteProtectedPath(int number)
    {
        var sequence = _filePathProviderMock
            .SetupSequence(m => m.DoesFileExist(It.IsAny<string>()));

        for (int i = 0; i < number; i++)
        {
            sequence.Returns(true);
        }
        sequence.Returns(false);
    }

    [Theory, MemberData(nameof(GetFileNameWithExtensionTestData))]
    public void GetFileNameWithExtension_WhenPathIsValid_ReturnsFileNameWithExtension(string path, string expected)
    {
        Assert.Equal(expected, _filePathProviderMock.Object.GetFileNameWithExtension(path));
    }

    public static IEnumerable<object[]> GetFileNameWithExtensionTestData() =>
    [
        [P("file_that_exists.txt"), "file_that_exists.txt"],
            [P("config.json.sample"), "config.json.sample"],
            [P("config"), "config"],
            [P("test.txt.txt"), "test.txt.txt"],
            [P("nest1", "nest2", "file_that_exists.txt"), "file_that_exists.txt"],
            [P("nest1", "nest2", "config.json.sample"), "config.json.sample"],
            [P("nest1", "nest2", "config"), "config"],
            [P("nest1", "nest2", "test.txt.txt"), "test.txt.txt"]
    ];

    [Theory, MemberData(nameof(GetFileNameWithoutExtensionTestData))]
    public void GetFileNameWithoutExtension_WhenPathIsValid_ReturnsFileNameWithoutExtension(string path, string expected)
    {
        Assert.Equal(expected, _filePathProviderMock.Object.GetFileNameWithoutExtension(path));
    }

    public static IEnumerable<object[]> GetFileNameWithoutExtensionTestData() =>
    [
        [P("file_that_exists.txt"), "file_that_exists"],
            [P("config.json.sample"), "config.json"],
            [P("config"), "config"],
            [P("test.txt.txt"), "test.txt"],
            [P("nest1", "nest2", "file_that_exists.txt"), "file_that_exists"],
            [P("nest1", "nest2", "config.json.sample"), "config.json"],
            [P("nest1", "nest2", "config"), "config"],
            [P("nest1", "nest2", "test.txt.txt"), "test.txt"]
    ];
}
