using System;
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

        private static BattleScribeXmlSerializer Serializer
            => BattleScribeXmlSerializer.Instance;

        public static VersionedElementInfo ReadRootElementInfo(Stream stream)
        {
            var settings = new XmlReaderSettings() { CloseInput = false };
            using (var reader = XmlReader.Create(stream, settings))
            {
                return ReadRootElementInfo(reader);
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

        public static (XmlReader reader, VersionedElementInfo info) ReadMigrated(
            XmlReader inputReader)
        {
            var info = ReadRootElementInfo(inputReader);
            var migrations = info.AvailableMigrations();
            var migrated =
                migrations.Aggregate(
                    inputReader,
                    (previous, migration) =>
                    {
                        using (previous)
                        {
                            var resultStream = new MemoryStream();
                            ApplyMigration(migration, previous, resultStream);
                            resultStream.Position = 0;
                            return XmlReader.Create(resultStream);
                        }
                    });
            var migratedVersionInfo = migrations.Count > 0 ? migrations.Last() : info;
            return (migrated, migratedVersionInfo);
        }

        public static (XmlReader reader, VersionedElementInfo info) ReadMigrated(Stream input)
        {
            var settings = new XmlReaderSettings() { CloseInput = false };
            var inputReader = XmlReader.Create(input, settings);
            return ReadMigrated(inputReader);
        }

        public static VersionedElementInfo WriteMigrated(Stream input, Stream output)
        {
            var settings = new XmlReaderSettings() { CloseInput = false };
            using (var inputReader = XmlReader.Create(input, settings))
            {
                var (reader, info) = ReadMigrated(inputReader);
                var sourceNode = BattleScribeXmlSerializer.Instance
                    .Deserialize(x => x.Deserialize(reader), info.Element);
                sourceNode.Serialize(output);
                return info;
            }
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

        public static SourceNode DeserializeMigrated(Stream input)
        {
            var (reader, info) = ReadMigrated(input);
            using (reader)
            {
                return Serializer.Deserialize(x => x.Deserialize(reader), info.Element);
            }
        }

        public static SourceNode DeserializeAuto(
            Stream stream,
            MigrationMode mode = MigrationMode.None)
        {
            return mode switch
            {
                MigrationMode.None => DeserializeSimple(stream),
                MigrationMode.OnFailure => WithSeekable(seekable =>
                      {
                          try
                          {
                              return DeserializeSimple(seekable);
                          }
                          catch (InvalidOperationException)
                          {
                              return DeserializeMigrated(seekable);
                          }
                      }),
                MigrationMode.Always => WithSeekable(DeserializeMigrated),
                _ => throw new ArgumentException(
                        $"Invalid {nameof(MigrationMode)} value.",
                        nameof(mode)),
            };
            SourceNode DeserializeSimple(Stream source)
            {
                using (var reader = XmlReader.Create(source))
                {
                    var rootInfo = ReadRootElementInfo(reader);
                    return Serializer.Deserialize(x => x.Deserialize(reader), rootInfo.Element);
                }
            }
            SourceNode WithSeekable(Func<Stream, SourceNode> func)
            {
                if (stream.CanSeek)
                {
                    return func(stream);
                }
                else
                {
                    using (var memory = new MemoryStream())
                    {
                        stream.CopyTo(memory);
                        memory.Position = 0;
                        return func(memory);
                    }
                }
            }
        }
    }
}
