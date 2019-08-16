using System.IO;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    /// <summary>
    /// Provides extension methods that wrap methods from <see cref="BattleScribeXmlSerializer"/>.
    /// </summary>
    public static class BattleScribeSerializationExtensions
    {
        private static BattleScribeXmlSerializer Serializer
            => BattleScribeXmlSerializer.Instance;

        public static GamesystemNode DeserializeGamesystem(this Stream stream)
        {
            return Serializer.DeserializeGamesystem(x => x.Deserialize(stream));
        }

        public static CatalogueNode DeserializeCatalogue(this Stream stream)
        {
            return Serializer.DeserializeCatalogue(x => x.Deserialize(stream));
        }

        public static RosterNode DeserializeRoster(this Stream stream)
        {
            return Serializer.DeserializeRoster(x => x.Deserialize(stream));
        }

        public static DataIndexNode DeserializeDataIndex(this Stream stream)
        {
            return Serializer.DeserializeDataIndex(x => x.Deserialize(stream));
        }

        public static SourceNode DeserializeSourceNodeAuto(
            this Stream stream,
            MigrationMode mode = MigrationMode.None)
        {
            return DataVersionManagement.DeserializeAuto(stream, mode);
        }

        public static void Serialize(this SourceNode node, Stream stream)
        {
            Serializer.Serialize(node, stream);
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
