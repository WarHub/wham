using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    /// <summary>
    /// Provides extension methods that wrap methods from <see cref="BattleScribeXmlSerializer"/>.
    /// </summary>
    public static class BattleScribeSerializationExtensions
    {
        private static Lazy<BattleScribeXmlSerializer> BSSerializer { get; }
            = new Lazy<BattleScribeXmlSerializer>();

        private static BattleScribeXmlSerializer Serializer => BSSerializer.Value;

        public static GamesystemNode DeserializeGamesystem(this Stream stream)
        {
            return Serializer.DeserializeGamesystem(stream);
        }

        public static CatalogueNode DeserializeCatalogue(this Stream stream)
        {
            return Serializer.DeserializeCatalogue(stream);
        }

        public static RosterNode DeserializeRoster(this Stream stream)
        {
            return Serializer.DeserializeRoster(stream);
        }

        public static DataIndexNode DeserializeDataIndex(this Stream stream)
        {
            return Serializer.DeserializeDataIndex(stream);
        }

        public static SourceNode DeserializeAuto(
            this Stream stream,
            BsDeserializationMode mode = BsDeserializationMode.Simple)
        {
            switch (mode)
            {
                case BsDeserializationMode.Simple:
                    return DeserializeSimple(stream);
                case BsDeserializationMode.MigrateOnFailure:
                    return WithSeekable(seekable =>
                    {
                        try
                        {
                            return DeserializeSimple(seekable);
                        }
                        catch (Exception)
                        {
                            return DeserializeMigrating(seekable);
                        }
                    });
                case BsDeserializationMode.MigrateAlways:
                    return WithSeekable(DeserializeMigrating);
                default:
                    throw new ArgumentException("Invalid mode.", nameof(mode));
            }
            SourceNode DeserializeSimple(Stream source)
            {
                using (var reader = XmlReader.Create(source))
                {
                    var rootInfo = DataVersionManagement.ReadRootElementInfo(reader);
                    return Deserialize(x => x.Deserialize(reader), rootInfo.Element);
                }
            }
            SourceNode DeserializeMigrating(Stream source)
            {
                var (migrated, info) =
                    DataVersionManagement.Migrate(
                        () =>
                        {
                            source.Position = 0;
                            return source;
                        });
                using (migrated)
                {
                    return Deserialize(migrated, info.Element);
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

        private static SourceNode Deserialize(Stream stream, RootElement rootElement)
        {
            return Deserialize(x => x.Deserialize(stream), rootElement);
        }

        private static SourceNode Deserialize(
            Func<XmlSerializer, object> deserialization,
            RootElement rootElement)
        {
            switch (rootElement)
            {
                case RootElement.Catalogue:
                    return Serializer.DeserializeCatalogue(deserialization);
                case RootElement.GameSystem:
                    return Serializer.DeserializeGamesystem(deserialization);
                case RootElement.Roster:
                    return Serializer.DeserializeRoster(deserialization);
                case RootElement.DataIndex:
                    return Serializer.DeserializeDataIndex(deserialization);
                default:
                    throw new ArgumentException(
                        $"Deserialization is not supported for this {nameof(RootElement)}.",
                        nameof(rootElement));
            }
        }

        public static void Serialize(this SourceNode node, Stream stream)
        {
            switch (node.Kind)
            {
                case SourceKind.Gamesystem:
                    Serializer.SerializeGamesystem((GamesystemNode)node, stream);
                    return;
                case SourceKind.Catalogue:
                    Serializer.SerializeCatalogue((CatalogueNode)node, stream);
                    return;
                case SourceKind.Roster:
                    Serializer.SerializeRoster((RosterNode)node, stream);
                    return;
                case SourceKind.DataIndex:
                    Serializer.SerializeDataIndex((DataIndexNode)node, stream);
                    return;
                default:
                    throw new ArgumentException(
                        $"{nameof(node)} type's ({node?.GetType()}) serialization is not supported.",
                        nameof(node));
            }
        }

        public static GamesystemCore.FastSerializationProxy GetSerializationProxy(this GamesystemNode node)
        {
            return node.Core.ToSerializationProxy();
        }

        public static CatalogueCore.FastSerializationProxy GetSerializationProxy(this CatalogueNode node)
        {
            return node.Core.ToSerializationProxy();
        }

        public static RosterCore.FastSerializationProxy GetSerializationProxy(this RosterNode node)
        {
            return node.Core.ToSerializationProxy();
        }

        public static DataIndexCore.FastSerializationProxy GetSerializationProxy(this DataIndexNode node)
        {
            return node.Core.ToSerializationProxy();
        }
    }

    public enum BsDeserializationMode
    {
        /// <summary>
        /// This mode means that no additional action will be taken aside of deserialization.
        /// </summary>
        Simple,

        /// <summary>
        /// In this mode if the first deserialization fails, migrations (if available) will be applied.
        /// </summary>
        MigrateOnFailure,

        /// <summary>
        /// In this mode, migrations (if available) are applied.
        /// </summary>
        MigrateAlways,
    }
}
