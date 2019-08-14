using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            var settings = new XmlReaderSettings() { CloseInput = false };
            using (var reader = XmlReader.Create(stream, settings))
            {
                reader.MoveToContent();
                var rootElement = reader.LocalName.ParseRootElement();
                var versionText = reader.GetAttribute(BattleScribeVersionAttributeName);
                var version = BattleScribeVersion.Parse(versionText);
                return new VersionedElementInfo(rootElement, version);
            }
        }

        public static VersionedElementInfo ReadRootElementInfo(XmlReader reader)
        {
            reader.MoveToContent();
            var rootElement = reader.LocalName.ParseRootElement();
            var versionText = reader.GetAttribute(BattleScribeVersionAttributeName);
            var version = BattleScribeVersion.Parse(versionText);
            return new VersionedElementInfo(rootElement, version);
        }

        public static (Stream output, VersionedElementInfo inputInfo)  Migrate(
            Func<Stream> inputProvider,
            Func<Stream> cacheProvider = null)
        {
            if (inputProvider == null)
                throw new ArgumentNullException(nameof(inputProvider));

            return
                MigrateAsync(
                    () => Task.FromResult(inputProvider()),
                    () => Task.FromResult(cacheProvider is null ? new MemoryStream() : cacheProvider()))
                .GetAwaiter()
                .GetResult();
        }

        private static XmlReader ReadMigrated(Stream input)
        {
            var settings = new XmlReaderSettings() { CloseInput = false };
            var inputReader = XmlReader.Create(input, settings);
            var info = ReadRootElementInfo(inputReader);
            var migrations = info.AvailableMigrations();
            return
                migrations.Aggregate(
                    inputReader,
                    (previous, migration) =>
                    {
                        using (previous)
                        {
                            var resultStream = new MemoryStream();
                            ApplyMigration(migration, previous, resultStream);
                            resultStream.Position = 0;
                            return XmlReader.Create(resultStream, settings);
                        }
                    });
        }

        private static async Task<(Stream output, VersionedElementInfo inputInfo)> MigrateAsync(
            Func<Task<Stream>> inputProviderAsync,
            Func<Task<Stream>> cacheProviderAsync = null)
        {
            if (inputProviderAsync == null)
                throw new ArgumentNullException(nameof(inputProviderAsync));
            using (var originStream = await inputProviderAsync())
            using (var originReader = XmlReader.Create(originStream))
            {
                var info = ReadRootElementInfo(originReader);
                var migrations = info.AvailableMigrations();
                var result = await migrations
                    .Aggregate(
                        Task.FromResult(originStream),
                        async (previousTask, migration) =>
                        {
                            using (var previousStream = await previousTask)
                            {
                                var resultStream = await GetCacheAsync();
                                ApplyMigration(migration, previousStream, resultStream);
                                resultStream.Position = 0;
                                return resultStream;
                            }
                        });
                return (result, info);
            }
            async Task<Stream> GetCacheAsync()
                => cacheProviderAsync is null
                    ? new MemoryStream()
                    : await cacheProviderAsync();
        }

        public static void ApplyMigration(VersionedElementInfo migrationInfo, XmlReader input, Stream output)
        {
            var xslt = CreateXslt();
            var writer = Utilities.BattleScribeConformantXmlWriter.Create(output);
            xslt.Transform(input, writer);

            XslCompiledTransform CreateXslt()
            {
                using (var migrationXlsStream = migrationInfo.OpenMigrationXslStream())
                {
                    var transform = new XslCompiledTransform();
                    transform.Load(XmlReader.Create(migrationXlsStream));
                    return transform;
                }
            }
        }

        public static void ApplyMigration(VersionedElementInfo migrationInfo, Stream input, Stream output)
        {
            var settings = new XmlReaderSettings() { CloseInput = false };
            ApplyMigration(migrationInfo, XmlReader.Create(input, settings), output);
        }
    }
}
