﻿namespace Corely.Common.File;

public class FilePathProvider : IFilePathProvider
{
    public virtual bool DoesFileExist(string filepath)
    {
        return System.IO.File.Exists(filepath);
    }

    public string GetOverwriteProtectedPath(string filepath)
    {
        if (!DoesFileExist(filepath)) { return filepath; }

        FileInfo info = new(filepath);
        FileInfo newInfo;
        int i = 0;
        do
        {
            newInfo = new(
                Path.Combine(
                    info.DirectoryName ?? string.Empty,
                    info.NumberedName(++i)
                )
            );
        }
        while (DoesFileExist(newInfo.FullName));

        return newInfo.FullName;
    }

    public string GetFileNameWithExtension(string filepath)
    {
        var info = new FileInfo(filepath);
        return info.Name;
    }

    public string GetFileNameWithoutExtension(string filepath)
    {
        var info = new FileInfo(filepath);
        return info.NameWithoutExtension();
    }
}
