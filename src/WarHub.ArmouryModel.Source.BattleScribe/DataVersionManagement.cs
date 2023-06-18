using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Xsl;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    public static class DataVersionManagement
    {
        public const string BattleScribeVersionAttributeName = "battleScribeVersion";

        private static BattleScribeXmlSerializer Serializer
            => BattleScribeXmlSerializer.Instance;

        public static VersionedElementInfo ReadRootElementInfo(Stream stream, CancellationToken cancellationToken = default)
        {
            var settings = new XmlReaderSettings() { CloseInput = false };
            using var reader = XmlReader.Create(stream, settings);
            return ReadRootElementInfo(reader, cancellationToken);
        }

        public static VersionedElementInfo ReadRootElementInfo(XmlReader reader, CancellationToken cancellationToken = default)
        {
            reader.MoveToContent();
            var rootElement = reader.LocalName.ParseRootElement();
            var versionText = reader.GetAttribute(BattleScribeVersionAttributeName)
                ?? throw new InvalidOperationException(BattleScribeVersionAttributeName + " attribute not found on root element.");
            var version = BattleScribeVersion.Parse(versionText);
            return new VersionedElementInfo(rootElement, version);
        }

        public static (XmlReader reader, VersionedElementInfo info) ReadMigrated(
            XmlReader inputReader,
            CancellationToken cancellationToken = default)
        {
            var info = ReadRootElementInfo(inputReader, cancellationToken);
            var migrations = info.AvailableMigrations();
            var migratedReader =
                migrations.Aggregate(
                    inputReader,
                    (previous, migration) =>
                    {
                        using (previous)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var resultStream = new MemoryStream();
                            ApplyMigration(migration, previous, resultStream);
                            resultStream.Position = 0;
                            return XmlReader.Create(resultStream);
                        }
                    });
            var migratedVersionInfo = migrations.Count > 0 ? migrations.Last() : info;
            return (migratedReader, migratedVersionInfo);
        }

        public static (XmlReader reader, VersionedElementInfo info) ReadMigrated(Stream input, CancellationToken cancellationToken = default)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            // created reader will be either returned or disposed of in this method:
            return ReadMigrated(XmlReader.Create(input), cancellationToken);
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        public static VersionedElementInfo WriteMigrated(Stream input, Stream output)
        {
            var settings = new XmlReaderSettings() { CloseInput = false };
            using var inputReader = XmlReader.Create(input, settings);
            var (reader, info) = ReadMigrated(inputReader);
            var sourceNode = BattleScribeXmlSerializer.Instance
                .Deserialize(x => x.Deserialize(reader), info.Element);
            sourceNode?.Serialize(output);
            return info;
        }

        public static void ApplyMigration(VersionedElementInfo migrationInfo, XmlReader input, Stream output, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var xslt = CreateXslt();
            using var writer = Utilities.BattleScribeConformantXmlWriter.Create(output, new XmlWriterSettings { CloseOutput = false });
            xslt.Transform(input, writer);

            XslCompiledTransform CreateXslt()
            {
                using var migrationXlsStream = migrationInfo.OpenMigrationXslStream()
                    ?? throw new InvalidOperationException($"Failed to find migration XSL resource for {migrationInfo}.");
                using var stylesheetReader = XmlReader.Create(migrationXlsStream, new XmlReaderSettings { CloseInput = false });
                var transform = new XslCompiledTransform();
                transform.Load(stylesheetReader);
                return transform;
            }
        }

        public static SourceNode? DeserializeMigrated(Stream input, CancellationToken cancellationToken = default)
        {
            var (reader, info) = ReadMigrated(input, cancellationToken);
            using (reader)
            {
                return Serializer.Deserialize(x => x.Deserialize(reader), info.Element);
            }
        }

        public static SourceNode? DeserializeAuto(
            Stream stream,
            MigrationMode mode = MigrationMode.None,
            CancellationToken cancellationToken = default)
        {
            return mode switch
            {
                MigrationMode.None => DeserializeSimple(stream, cancellationToken),
                MigrationMode.OnFailure => WithSeekable(stream, static (seekable, cancellationToken) =>
                      {
                          try
                          {
                              return DeserializeSimple(seekable, cancellationToken);
                          }
                          catch (InvalidOperationException)
                          {
                              return DeserializeMigrated(seekable, cancellationToken);
                          }
                      }, cancellationToken),
                MigrationMode.Always => WithSeekable(stream, DeserializeMigrated, cancellationToken),
                _ => throw new ArgumentException(
                        $"Invalid {nameof(MigrationMode)} value.",
                        nameof(mode)),
            };
            static SourceNode? DeserializeSimple(Stream source, CancellationToken cancellationToken = default)
            {
                using var reader = XmlReader.Create(source);
                var rootInfo = ReadRootElementInfo(reader, cancellationToken);
                return Serializer.Deserialize(x => x.Deserialize(reader), rootInfo.Element);
            }
            static SourceNode? WithSeekable(Stream stream, Func<Stream, CancellationToken, SourceNode?> func, CancellationToken cancellationToken = default)
            {
                if (stream.CanSeek)
                {
                    return func(stream, cancellationToken);
                }
                else
                {
                    using var memory = new MemoryStream();
                    stream.CopyTo(memory);
                    memory.Position = 0;
                    return func(memory, cancellationToken);
                }
            }
        }
    }
}
