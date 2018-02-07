using System.IO;
using System.Xml;

namespace WarHub.ArmouryModel.Source.Tests
{
    public sealed class BattleScribeConformantXmlWriter : XmlWriter
    {
        public BattleScribeConformantXmlWriter(XmlWriter writer)
        {
            BaseWriter = writer;
        }

        private XmlWriter BaseWriter { get; }

        public new static XmlWriter Create(Stream stream, XmlWriterSettings settings)
        {
            return new BattleScribeConformantXmlWriter(XmlWriter.Create(stream, settings));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseWriter?.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void WriteString(string text)
        {
            switch (WriteState)
            {
                case WriteState.Attribute:
                case WriteState.Element:
                    {
                        var raw = BattleScribeXml.Encoder.Encode(text);
                        BaseWriter.WriteRaw(raw);
                    }
                    break;
                default:
                    BaseWriter.WriteString(text);
                    break;
            }
        }

        public override void WriteStartDocument() => WriteBattleScribeStartDocument();
        public override void WriteStartDocument(bool standalone) => WriteBattleScribeStartDocument();

        private void WriteBattleScribeStartDocument()
        {
            BaseWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
        }

        #region forwarding overrides to BaseWriter

        public override WriteState WriteState => BaseWriter.WriteState;
        public override void Flush() => BaseWriter.Flush();
        public override string LookupPrefix(string ns) => BaseWriter.LookupPrefix(ns);
        public override void WriteBase64(byte[] buffer, int index, int count) => BaseWriter.WriteBase64(buffer, index, count);
        public override void WriteCData(string text) => BaseWriter.WriteCData(text);
        public override void WriteCharEntity(char ch) => BaseWriter.WriteCharEntity(ch);
        public override void WriteChars(char[] buffer, int index, int count) => BaseWriter.WriteChars(buffer, index, count);
        public override void WriteComment(string text) => BaseWriter.WriteComment(text);
        public override void WriteDocType(string name, string pubid, string sysid, string subset) => BaseWriter.WriteDocType(name, pubid, sysid, subset);
        public override void WriteEndAttribute() => BaseWriter.WriteEndAttribute();
        public override void WriteEndDocument() => BaseWriter.WriteEndDocument();
        public override void WriteEndElement() => BaseWriter.WriteEndElement();
        public override void WriteEntityRef(string name) => BaseWriter.WriteEntityRef(name);
        public override void WriteFullEndElement() => BaseWriter.WriteFullEndElement();
        public override void WriteProcessingInstruction(string name, string text) => BaseWriter.WriteProcessingInstruction(name, text);
        public override void WriteRaw(char[] buffer, int index, int count) => BaseWriter.WriteRaw(buffer, index, count);
        public override void WriteRaw(string data) => BaseWriter.WriteRaw(data);
        public override void WriteStartAttribute(string prefix, string localName, string ns) => BaseWriter.WriteStartAttribute(prefix, localName, ns);
        public override void WriteStartElement(string prefix, string localName, string ns) => BaseWriter.WriteStartElement(prefix, localName, ns);
        public override void WriteSurrogateCharEntity(char lowChar, char highChar) => BaseWriter.WriteSurrogateCharEntity(lowChar, highChar);
        public override void WriteWhitespace(string ws) => BaseWriter.WriteWhitespace(ws);

        #endregion
    }
}
