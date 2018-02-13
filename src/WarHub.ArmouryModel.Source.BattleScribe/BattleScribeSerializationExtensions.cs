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

        public static GamesystemNode DeserializeGamesystem(this Stream stream)
        {
            return BSSerializer.Value.DeserializeGamesystem(stream);
        }

        public static CatalogueNode DeserializeCatalogue(this Stream stream)
        {
            return BSSerializer.Value.DeserializeCatalogue(stream);
        }

        public static RosterNode DeserializeRoster(this Stream stream)
        {
            return BSSerializer.Value.DeserializeRoster(stream);
        }

        public static DataIndexNode DeserializeDataIndex(this Stream stream)
        {
            return BSSerializer.Value.DeserializeDataIndex(stream);
        }

        public static void Serialize(this GamesystemNode node, Stream stream)
        {
            BSSerializer.Value.SerializeGamesystem(node, stream);
        }

        public static void Serialize(this CatalogueNode node, Stream stream)
        {
            BSSerializer.Value.SerializeCatalogue(node, stream);
        }

        public static void Serialize(this RosterNode node, Stream stream)
        {
            BSSerializer.Value.SerializeRoster(node, stream);
        }

        public static void Serialize(this DataIndexNode node, Stream stream)
        {
            BSSerializer.Value.SerializeDataIndex(node, stream);
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
