using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Xsl;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    public static class DataVersionManagement
    {
        public const string BattleScribeVersionAttributeName = "battleScribeVersion";

        public static (XmlInformation.RootElement, string rootNamespace, XmlInformation.BsDataVersion)
            ReadDocumentInfo(Stream stream)
        {
            using (var reader = XmlReader.Create(stream))
            {
                reader.MoveToContent();
                var rootElement = reader.LocalName.ParseRootElement();
                var versionAttribute = reader.GetAttribute(BattleScribeVersionAttributeName);
                var version = versionAttribute.ParseBsDataVersion();
                return (rootElement, reader.NamespaceURI, version);
            }
        }

        public static Stream ApplyMigrations(Stream input)
        {
            var stream = GetSeekableStream(input);
            var (rootElement, _, dataVersion) = ReadDocumentInfo(stream);
            stream.Position = 0;
            return
                rootElement.Info()
                .AvailableMigrations(dataVersion)
                .Aggregate(
                    stream,
                    (acc, version) =>
                    {
                        using (acc)
                        {
                            return ApplyMigration(acc, rootElement, version);
                        }
                    });

            Stream GetSeekableStream(Stream source)
            {
                if (source.CanSeek)
                {
                    return source;
                }
                var memory = new MemoryStream();
                source.CopyTo(memory);
                memory.Position = 0;
                return memory;
            }
        }

        public static Stream ApplyMigration(
            Stream inputData,
            XmlInformation.RootElement rootElement,
            XmlInformation.BsDataVersion targetVersion)
        {
            var xslt = CreateXslt();
            var result = new MemoryStream();
            var writer = Utilities.BattleScribeConformantXmlWriter.Create(result);
            xslt.Transform(XmlReader.Create(inputData), writer);
            result.Position = 0;
            return result;

            XslCompiledTransform CreateXslt()
            {
                using (var migrationXlsStream = XmlInformation.OpenMigrationXslStream(rootElement, targetVersion))
                {
                    var transform = new XslCompiledTransform();
                    transform.Load(XmlReader.Create(migrationXlsStream));
                    return transform;
                }
            }
        }
    }
}
