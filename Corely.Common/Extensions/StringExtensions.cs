using System.Text.RegularExpressions;

namespace Corely.Common.Extensions;

public static class StringExtensions
{
    extension(string s)
    {
        public string Base64Encode()
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s));
        }

        public string Base64Decode()
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }

        public string UrlEncode()
        {
            ArgumentNullException.ThrowIfNull(s);

            return Uri.EscapeDataString(s);
        }

        public string UrlDecode()
        {
            ArgumentNullException.ThrowIfNull(s);

            return Uri.UnescapeDataString(s);
        }

        internal string WithJsonFieldsOmitted(string[] fields)
        {
            if (fields.Length == 0)
                return s;

            var alternation = string.Join("|", fields.Select(Regex.Escape));
            var pattern = $@"""({alternation})""(\s*):(\s*)""[^""]*""";

            return Regex.Replace(
                s,
                pattern,
                m => $"\"{m.Groups[1].Value}\"{m.Groups[2].Value}:{m.Groups[3].Value}\"[OMITTED]\"",
                RegexOptions.CultureInvariant | RegexOptions.Singleline
            );
        }

        internal string WithJsonFieldTruncated(string field, int maxLength)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(field) || maxLength < 0)
                return s;

            var escapedField = Regex.Escape(field);
            var pattern = $@"""({escapedField})""(\s*):(\s*)""([^""]*)""";

            return Regex.Replace(
                s,
                pattern,
                m =>
                {
                    var value = m.Groups[4].Value;
                    var truncated = value.Length > maxLength ? value[..maxLength] : value;
                    var suffix = value.Length > maxLength ? "...[TRUNCATED]" : string.Empty;
                    return $"\"{m.Groups[1].Value}\"{m.Groups[2].Value}:{m.Groups[3].Value}\"{truncated}{suffix}\"";
                },
                RegexOptions.CultureInvariant | RegexOptions.Singleline
            );
        }
    }
}
