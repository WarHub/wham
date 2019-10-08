using System.Text;

namespace WarHub.ArmouryModel.Source.BattleScribe.Utilities
{
    /// <summary>
    /// Encodes XML element text and attribute text in a format 100% conformant with BattleScribe output.
    /// This means that every one of the five entity-escapable characters is escaped:
    /// &amp;, &quot;, &apos;, &gt;, and &lt;
    /// </summary>
    public static class BattleScribeXmlEncoder
    {
        private const char Amp = '&';
        private const char Lt = '<';
        private const char Gt = '>';
        private const char Apos = '\'';
        private const char Quot = '"';
        private const char Cr = '\r';
        private const char Lf = '\n';
        private const string AmpString = "&amp;";
        private const string LtString = "&lt;";
        private const string GtString = "&gt;";
        private const string QuotString = "&quot;";
        private const string AposString = "&apos;";
        private static readonly char[] xmlEscaped = new[] { Amp, Lt, Gt, Apos, Quot };

        /// <summary>
        /// Escapes all XML entity-escapable characters and replaces CRLF with just LF.
        /// </summary>
        /// <param name="text">Text to be escaped.</param>
        /// <returns>XML-escaped text with LF line endings.</returns>
        public static string Encode(string text)
        {
            var sb = new StringBuilder(text.Length);
            var noEscaped = text.IndexOfAny(xmlEscaped) < 0;
            if (noEscaped)
            {
                return text;
            }
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                switch (c)
                {
                    case Amp:
                        sb.Append(AmpString);
                        break;
                    case Lt:
                        sb.Append(LtString);
                        break;
                    case Gt:
                        sb.Append(GtString);
                        break;
                    case Quot:
                        sb.Append(QuotString);
                        break;
                    case Apos:
                        sb.Append(AposString);
                        break;
                    case Cr:
                        {
                            var nextIndex = i + 1;
                            if (nextIndex < text.Length && text[nextIndex] == Lf)
                            {
                                // we've got CR LF
                                // skip LF
                                i = nextIndex;
                                // normalize to LF
                                sb.Append(Lf);
                            }
                            else
                            {
                                sb.Append(Cr);
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
