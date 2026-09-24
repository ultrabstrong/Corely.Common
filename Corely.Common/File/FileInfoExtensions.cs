namespace Corely.Common.File;

internal static class FileInfoExtensions
{
    extension(FileInfo info)
    {
        public string NameWithoutExtension()
        {
            int place = info.Name.LastIndexOf(info.Extension);
            if (place == -1)
            {
                return info.Name;
            }
            return info.Name.Remove(place, info.Extension.Length);
        }

        public string NumberedName(int number)
        {
            if (string.IsNullOrWhiteSpace(info.Extension))
            {
                return $"{info.Name}-[{number}]";
            }
            return $"{info.NameWithoutExtension()}-[{number}]{info.Extension}";
        }
    }
}
