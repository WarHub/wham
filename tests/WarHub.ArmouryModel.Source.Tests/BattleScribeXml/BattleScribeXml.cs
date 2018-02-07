using System.IO;
using System.Text;
using System.Xml;

namespace WarHub.ArmouryModel.Source.Tests
{
    public static class BattleScribeXml
    {
        public static XmlWriterSettings XmlWriterSettings => InternalXmlWriterSettings.Clone();

        static XmlWriterSettings InternalXmlWriterSettings { get; } = new XmlWriterSettings
        {
            CloseOutput = false,
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace,
            Indent = true,
            OmitXmlDeclaration = false
        };

        public static void WriteBattleScribeStartDocument(this Stream stream)
        {
            var text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n";
            var writer = new StreamWriter(stream) { AutoFlush = true };
            writer.Write(text);
        }
        public static class Encoder
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
}
