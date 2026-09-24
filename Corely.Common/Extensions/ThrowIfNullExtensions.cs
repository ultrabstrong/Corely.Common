namespace Corely.Common.Extensions;

public static class ThrowIfNullExtensions
{
    extension<T>(T? obj)
        where T : class
    {
        public T ThrowIfNull(string paramName)
        {
            ArgumentNullException.ThrowIfNull(obj, paramName);
            return obj;
        }
    }

    extension<T>(IEnumerable<T?>? obj)
        where T : class
    {
        public IEnumerable<T> ThrowIfAnyNull(string paramName)
        {
            ArgumentNullException.ThrowIfNull(obj, paramName);
            foreach (var value in obj)
            {
                ArgumentNullException.ThrowIfNull(value, paramName);
            }
            return obj!;
        }
    }

    extension(string? str)
    {
        public string ThrowIfNullOrWhiteSpace(string paramName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(str, paramName);
            return str;
        }

        public string ThrowIfNullOrEmpty(string paramName)
        {
            ArgumentException.ThrowIfNullOrEmpty(str, paramName);
            return str;
        }
    }

    extension(IEnumerable<string>? obj)
    {
        public IEnumerable<string> ThrowIfAnyNullOrWhiteSpace(string paramName)
        {
            ArgumentNullException.ThrowIfNull(obj, paramName);
            foreach (var value in obj)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
            }
            return obj;
        }

        public IEnumerable<string> ThrowIfAnyNullOrEmpty(string paramName)
        {
            ArgumentNullException.ThrowIfNull(obj, paramName);
            foreach (var value in obj)
            {
                ArgumentException.ThrowIfNullOrEmpty(value, paramName);
            }
            return obj;
        }
    }
}
