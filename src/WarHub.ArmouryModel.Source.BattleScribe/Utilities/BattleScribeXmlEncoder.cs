using System.Text;

namespace WarHub.ArmouryModel.Source.BattleScribe.Utilities
{
    /// <summary>
    /// Encodes XML element text and attribute text in a format 100% conformant with BattleScribe output.
    /// This means that every one of the five entity-escapable characters is escaped:
    /// &amp;, &quot;, &apos;, &gt;, and &lt;
    /// </summary>
    public static partial class BattleScribeXmlEncoder
    {
        const char amp = '&';
        const char lt = '<';
        const char gt = '>';
        const char apos = '\'';
        const char quot = '"';
        const char cr = '\r';
        const char lf = '\n';
        const string ampString = "&amp;";
        const string ltString = "&lt;";
        const string gtString = "&gt;";
        const string quotString = "&quot;";
        const string aposString = "&apos;";
        static readonly char[] xmlEscaped = new[] { amp, lt, gt, apos, quot };

        /// <summary>
        /// Escapes all XML entity-escapable characters and replaces CRLF with just LF.
        /// </summary>
        /// <param name="text">Text to be escaped.</param>
        /// <returns>XML-escaped text with LF line endings.</returns>
        public static string Encode(string text)
        {
            var sb = new StringBuilder(text.Length);
            bool noEscaped = text.IndexOfAny(xmlEscaped) < 0;
            if (noEscaped)
            {
                return text;
            }
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                switch (c)
                {
                    case amp:
                        sb.Append(ampString);
                        break;
                    case lt:
                        sb.Append(ltString);
                        break;
                    case gt:
                        sb.Append(gtString);
                        break;
                    case quot:
                        sb.Append(quotString);
                        break;
                    case apos:
                        sb.Append(aposString);
                        break;
                    case cr:
                        {
                            var nextIndex = i + 1;
                            if (nextIndex < text.Length && text[nextIndex] == lf)
                            {
                                // we've got CR LF
                                // skip LF
                                i = nextIndex;
                                // normalize to LF
                                sb.Append(lf);
                            }
                            else
                            {
                                sb.Append(cr);
                            }
                        }
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
