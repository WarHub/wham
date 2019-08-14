using System;
using System.IO;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    /// <summary>
    /// Provides extension methods that wrap methods from <see cref="BattleScribeXmlSerializer"/>.
    /// </summary>
    public static class BattleScribeSerializationExtensions
    {
        private static Lazy<BattleScribeXmlSerializer> BSSerializer { get; } = new Lazy<BattleScribeXmlSerializer>();

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

        public static SourceNode Deserialize(this Stream stream)
        {
            var seekableStream = GetSeekableStream(stream);
            var rootInfo = DataVersionManagement.ReadRootElementInfo(seekableStream);
            seekableStream.Position = 0;
            var sourceKind = rootInfo.RootElement.Info().SourceKind;
            return seekableStream.Deserialize(sourceKind);

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

        public static SourceNode Deserialize(this Stream stream, SourceKind sourceKind)
        {
            switch (sourceKind)
            {
                case SourceKind.Catalogue:
                    return stream.DeserializeCatalogue();
                case SourceKind.Gamesystem:
                    return stream.DeserializeGamesystem();
                case SourceKind.Roster:
                    return stream.DeserializeRoster();
                case SourceKind.DataIndex:
                    return stream.DeserializeDataIndex();
                default:
                    throw new ArgumentException(
                        $"Deserialization is not supported for this {nameof(SourceKind)}.",
                        nameof(sourceKind));
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
                    throw new ArgumentException($"{nameof(node)} type's ({node?.GetType()}) serialization is not supported.", nameof(node));
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
}
