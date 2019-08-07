using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public static async Task<Stream> ApplyMigrationsAsync(Stream input)
        {
            var stream = await GetSeekableStream(input);
            var (rootElement, _, dataVersion) = ReadDocumentInfo(stream);
            stream.Position = 0;
            if (dataVersion.Info().IsNewestVersion)
            {
                return stream;
            }
            var migrationVersions = dataVersion.Info().GetNewerVersions();
            return migrationVersions.Aggregate(
                stream,
                (prev, version) => ApplyMigration(prev, rootElement, version));

            async Task<Stream> GetSeekableStream(Stream source)
            {
                if (source.CanSeek)
                {
                    return source;
                }
                var memory = new MemoryStream();
                await source.CopyToAsync(memory);
                memory.Position = 0;
                return memory;
            }
        }

        public static Stream ApplyMigration(
            Stream previous,
            XmlInformation.RootElement rootElement,
            XmlInformation.BsDataVersion targetVersion)
        {
            using (previous)
            using (var migrationXlsStream = XmlInformation.OpenMigrationXslStream(rootElement, targetVersion))
            {
                var xslt = new XslCompiledTransform();
                xslt.Load(XmlReader.Create(migrationXlsStream));
                var result = new MemoryStream();
                var writer = Utilities.BattleScribeConformantXmlWriter.Create(result);
                xslt.Transform(XmlReader.Create(previous), writer);
                result.Position = 0;
                return result;
            }
        }
    }
}
