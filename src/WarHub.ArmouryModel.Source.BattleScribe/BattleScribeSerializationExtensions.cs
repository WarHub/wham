using System;
using System.IO;

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
                    throw new ArgumentException($"{nameof(node)} type's ({node?.GetType()}) serialization is not supported.");
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
