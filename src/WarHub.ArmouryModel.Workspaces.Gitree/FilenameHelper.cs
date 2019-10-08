using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    internal static class FilenameHelper
    {
        static FilenameHelper()
        {
            InvalidChars =
                Enumerable.Range(0, 32).Select(x => (char)x)
                .Concat(new[] { '"', '<', '>', '|', ':', '*', '?', '\\', '/' })
                .ToArray();
            var escaped = Regex.Escape(new string(InvalidChars));
            EscapingRegex = new Regex($@"[\s{escaped}]+");
        }

        private static Regex EscapingRegex { get; }
        private static char[] InvalidChars { get; }

        public static string FilenameSanitize(this string raw)
        {
            if (raw.IndexOfAny(InvalidChars) < 0)
            {
                return raw.Trim();
            }
            return EscapingRegex.Replace(raw, " ").Trim();
        }
    }
}
