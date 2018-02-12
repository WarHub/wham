using System;
using System.IO;
using System.Text;

namespace WarHub.ArmouryModel.Source.BattleScribe.Utilities
{
    /// <summary>
    /// Replaces " />" with "/>" on the fly. Fixes non-conforming formatting of XmlWriter.
    /// Doesn't support CDATA sections nor comments (will strip the space as well).
    /// </summary>
    public sealed class BattleScribeConformantTextWriter : TextWriter
    {
        public BattleScribeConformantTextWriter(TextWriter baseWriter)
        {
            BaseWriter = baseWriter;
        }

        const char space = ' ';
        const char slash = '/';
        const char gt = '>';
        const int smallestNonSpecial = gt + 1;
        private char? last;
        private readonly char[] pattern = new[] { ' ', '/', '>' };
        private readonly char[] patternToWrite = new[] { '/', '>' };

        public override Encoding Encoding => BaseWriter.Encoding;
        public override IFormatProvider FormatProvider => BaseWriter.FormatProvider;
        public override string NewLine { get => BaseWriter.NewLine; set => BaseWriter.NewLine = value; }

        private TextWriter BaseWriter { get; }

        public override void Write(char value)
        {
            if (value < smallestNonSpecial || value != space && value != slash && value != gt)
            {
                BaseWriter.Write(value);
                return;
            }
            // we have special value
            switch (last)
            {
                case null:
                    if (value == space)
                    {
                        last = space;
                        return;
                    }
                    break;
                case space:
                    if (value == slash)
                    {
                        last = slash;
                        return;
                    }
                    break;
                case slash:
                    if (value == gt)
                    {
                        last = null;
                        BaseWriter.Write(patternToWrite, 0, 2);
                        return;
                    }
                    break;
                default:
                    break;
            }
            last = null;
            BaseWriter.Write(value);
        }

        public override void Write(char[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            int offset = 0;
            while (last != null && offset < count)
            {
                Write(buffer[index + offset++]);
            }
            var countWithMargin = count - 3;
            var startingOffset = offset;
            while (offset < countWithMargin)
            {
                while (buffer[index + offset] != space && offset < countWithMargin)
                {
                    offset++;
                }
                // offset <= countWithMargin || offset < countWithMargin && buffer[index + offset] == space
                var current = index + offset;
                if (buffer[current] == space && buffer[current + 1] == slash && buffer[current + 2] == gt)
                {
                    // write since last startingOffset until current position, exclusive
                    BaseWriter.Write(buffer, index + startingOffset, offset - startingOffset);
                    // next batch will start after space
                    startingOffset = offset + 1;
                    // we mark '>' as "checked"
                    offset = offset + 2;
                }
                offset++;
            }
            BaseWriter.Write(buffer, index + startingOffset, (offset < count ? ++offset : count) - startingOffset);
            while (offset < count)
            {
                Write(buffer[index + offset++]);
            }
        }

        public override void Close() => BaseWriter.Close();
        public override void Flush() => BaseWriter.Flush();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseWriter.Dispose();
            }
        }
    }
}
