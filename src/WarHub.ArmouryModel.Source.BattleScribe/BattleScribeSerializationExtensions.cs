using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    /// <summary>
    /// Provides extension methods that wrap methods from <see cref="BattleScribeXmlSerializer"/>.
    /// </summary>
    public static class BattleScribeSerializationExtensions
    {
        private static BattleScribeXmlSerializer Serializer
            => BattleScribeXmlSerializer.Instance;

        public static GamesystemNode? DeserializeGamesystem(this Stream stream)
        {
            return Serializer.DeserializeGamesystem(DeserializeStreamFunc(stream));
        }

        public static CatalogueNode? DeserializeCatalogue(this Stream stream)
        {
            return Serializer.DeserializeCatalogue(DeserializeStreamFunc(stream));
        }

        public static RosterNode? DeserializeRoster(this Stream stream)
        {
            return Serializer.DeserializeRoster(DeserializeStreamFunc(stream));
        }

        public static DataIndexNode? DeserializeDataIndex(this Stream stream)
        {
            return Serializer.DeserializeDataIndex(DeserializeStreamFunc(stream));
        }

        private static Func<XmlSerializer, object?> DeserializeStreamFunc(Stream stream)
            => x => x.Deserialize(XmlReader.Create(stream));

        public static SourceNode? DeserializeSourceNodeAuto(
            this Stream stream,
            MigrationMode mode = MigrationMode.None)
        {
            return DataVersionManagement.DeserializeAuto(stream, mode);
        }

        public static void Serialize(this SourceNode node, Stream stream)
        {
            Serializer.Serialize(node, new StreamWriter(stream));
        }

        public static void Serialize(this SourceNode node, TextWriter writer)
        {
            Serializer.Serialize(node, writer);
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
