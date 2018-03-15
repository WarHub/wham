using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WarHub.ArmouryModel.CliTool.Utilities
{
    internal static class FilenameHelper
    {
        static FilenameHelper()
        {
            InvalidChars = new[] { '"', '<', '>', '|', ':', '*', '?', '\\', '/' };
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
