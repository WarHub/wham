using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Xsl;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    public static class DataVersionManagement
    {
        public const string BattleScribeVersionAttributeName = "battleScribeVersion";

        public static VersionedElementInfo ReadRootElementInfo(Stream stream)
        {
            using (var reader = XmlReader.Create(stream))
            {
                reader.MoveToContent();
                var rootElement = reader.LocalName.ParseRootElement();
                var versionText = reader.GetAttribute(BattleScribeVersionAttributeName);
                var version = BattleScribeVersion.Parse(versionText);
                return new VersionedElementInfo(rootElement, version);
            }
        }

        public static Stream ApplyMigrations(Stream input)
        {
            var stream = GetSeekableStream(input);
            var rootElementInfo = ReadRootElementInfo(stream);
            stream.Position = 0;
            return
                rootElementInfo.AvailableMigrations()
                .Aggregate(
                    stream,
                    (acc, next) =>
                    {
                        using (acc)
                        {
                            return ApplyMigration(acc, next);
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

        public static Stream ApplyMigration(Stream inputData, VersionedElementInfo elementInfo)
        {
            var xslt = CreateXslt();
            var result = new MemoryStream();
            var writer = Utilities.BattleScribeConformantXmlWriter.Create(result);
            xslt.Transform(XmlReader.Create(inputData), writer);
            result.Position = 0;
            return result;

            XslCompiledTransform CreateXslt()
            {
                using (var migrationXlsStream = elementInfo.OpenMigrationXslStream())
                {
                    var transform = new XslCompiledTransform();
                    transform.Load(XmlReader.Create(migrationXlsStream));
                    return transform;
                }
            }
        }
    }
}
