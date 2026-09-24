using System.Text;
using System.Text.RegularExpressions;

namespace Corely.Common.Extensions;

public static class RegexExtensions
{
    extension(Regex regex)
    {
        public string ReplaceGroups(string input, string replacement)
        {
            ArgumentNullException.ThrowIfNull(input);

            var sb = new StringBuilder(input.Length);
            var previousGroupEnd = 0;
            var inputSpan = input.AsSpan();

            foreach (Match match in regex.Matches(input))
            {
                foreach (Group group in match.Groups.Cast<Group>().Skip(1))
                {
                    if (group.Success)
                    {
                        sb.Append(inputSpan[previousGroupEnd..group.Index]);
                        sb.Append(replacement);
                        previousGroupEnd = group.Index + group.Length;
                    }
                }
            }

            if (previousGroupEnd < input.Length)
            {
                sb.Append(inputSpan[previousGroupEnd..]);
            }

            return sb.ToString();
        }
    }
}
