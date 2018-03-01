using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.BattleScribe.Utilities;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    /// <summary>
    /// Caches namespaces and serializers required for serialization and deserialization,
    /// and provides methods for performing those operations in strongly typed fashion.
    /// </summary>
    public class BattleScribeXmlSerializer
    {
        private Lazy<XmlSerializerNamespaces> LazyGamesystemNamespaces { get; }
            = new Lazy<XmlSerializerNamespaces>(CreateGamesystemXmlSerializerNamespaces);

        private Lazy<XmlSerializerNamespaces> LazyCatalogueNamespaces { get; }
            = new Lazy<XmlSerializerNamespaces>(CreateCatalogueXmlSerializerNamespaces);

        private Lazy<XmlSerializerNamespaces> LazyRosterNamespaces { get; }
            = new Lazy<XmlSerializerNamespaces>(CreateRosterXmlSerializerNamespaces);

        private Lazy<XmlSerializerNamespaces> LazyDataIndexNamespaces { get; }
            = new Lazy<XmlSerializerNamespaces>(CreateDataIndexXmlSerializerNamespaces);

        private Lazy<XmlSerializer> LazyGamesystemSerializer { get; }
            = new Lazy<XmlSerializer>(CreateGamesystemSerializer);

        private Lazy<XmlSerializer> LazyCatalogueSerializer { get; }
            = new Lazy<XmlSerializer>(CreateCatalogueSerializer);

        private Lazy<XmlSerializer> LazyRosterSerializer { get; }
            = new Lazy<XmlSerializer>(CreateRosterSerializer);

        private Lazy<XmlSerializer> LazyDataIndexSerializer { get; }
            = new Lazy<XmlSerializer>(CreateDataIndexSerializer);

        private Lazy<XmlSerializer> LazyGamesystemDeserializer { get; }
            = new Lazy<XmlSerializer>(CreateGamesystemDeserializer);

        private Lazy<XmlSerializer> LazyCatalogueDeserializer { get; }
            = new Lazy<XmlSerializer>(CreateCatalogueDeserializer);

        private Lazy<XmlSerializer> LazyRosterDeserializer { get; }
            = new Lazy<XmlSerializer>(CreateRosterDeserializer);

        private Lazy<XmlSerializer> LazyDataIndexDeserializer { get; }
            = new Lazy<XmlSerializer>(CreateDataIndexDeserializer);

        public GamesystemNode DeserializeGamesystem(Stream stream)
        {
            var builder = (GamesystemCore.Builder)LazyGamesystemDeserializer.Value.Deserialize(stream);
            return builder.ToImmutable().ToNode();
        }

        public CatalogueNode DeserializeCatalogue(Stream stream)
        {
            var builder = (CatalogueCore.Builder)LazyCatalogueDeserializer.Value.Deserialize(stream);
            return builder.ToImmutable().ToNode();
        }

        public RosterNode DeserializeRoster(Stream stream)
        {
            var builder = (RosterCore.Builder)LazyRosterDeserializer.Value.Deserialize(stream);
            return builder.ToImmutable().ToNode();
        }

        public DataIndexNode DeserializeDataIndex(Stream stream)
        {
            var builder = (DataIndexCore.Builder)LazyDataIndexDeserializer.Value.Deserialize(stream);
            return builder.ToImmutable().ToNode();
        }

        public void SerializeGamesystem(GamesystemNode node, Stream stream)
        {
            var fsp = node.GetSerializationProxy();
            using (var writer = BattleScribeConformantXmlWriter.Create(stream))
            {
                LazyGamesystemSerializer.Value.Serialize(writer, fsp, LazyGamesystemNamespaces.Value);
            }
        }

        public void SerializeCatalogue(CatalogueNode node, Stream stream)
        {
            var fsp = node.GetSerializationProxy();
            using (var writer = BattleScribeConformantXmlWriter.Create(stream))
            {
                LazyCatalogueSerializer.Value.Serialize(writer, fsp, LazyCatalogueNamespaces.Value);
            }
        }

        public void SerializeRoster(RosterNode node, Stream stream)
        {
            var fsp = node.GetSerializationProxy();
            using (var writer = BattleScribeConformantXmlWriter.Create(stream))
            {
                LazyRosterSerializer.Value.Serialize(writer, fsp, LazyRosterNamespaces.Value);
            }
        }

        public void SerializeDataIndex(DataIndexNode node, Stream stream)
        {
            var fsp = node.GetSerializationProxy();
            using (var writer = BattleScribeConformantXmlWriter.Create(stream))
            {
                LazyDataIndexSerializer.Value.Serialize(writer, fsp, LazyDataIndexNamespaces.Value);
            }
        }

        public static XmlSerializer CreateGamesystemSerializer() => new XmlSerializer(typeof(GamesystemCore.FastSerializationProxy));

        public static XmlSerializer CreateCatalogueSerializer() => new XmlSerializer(typeof(CatalogueCore.FastSerializationProxy));

        public static XmlSerializer CreateRosterSerializer() => new XmlSerializer(typeof(RosterCore.FastSerializationProxy));

        public static XmlSerializer CreateDataIndexSerializer() => new XmlSerializer(typeof(DataIndexCore.FastSerializationProxy));

        public static XmlSerializer CreateGamesystemDeserializer() => new XmlSerializer(typeof(GamesystemCore.Builder));

        public static XmlSerializer CreateCatalogueDeserializer() => new XmlSerializer(typeof(CatalogueCore.Builder));

        public static XmlSerializer CreateRosterDeserializer() => new XmlSerializer(typeof(RosterCore.Builder));

        public static XmlSerializer CreateDataIndexDeserializer() => new XmlSerializer(typeof(DataIndexCore.Builder));

        public static XmlSerializerNamespaces CreateGamesystemXmlSerializerNamespaces()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", GamesystemCore.GamesystemXmlNamespace);
            return namespaces;
        }

        public static XmlSerializerNamespaces CreateCatalogueXmlSerializerNamespaces()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", CatalogueCore.CatalogueXmlNamespace);
            return namespaces;
        }

        public static XmlSerializerNamespaces CreateRosterXmlSerializerNamespaces()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", RosterCore.RosterXmlNamespace);
            return namespaces;
        }

        public static XmlSerializerNamespaces CreateDataIndexXmlSerializerNamespaces()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", DataIndexCore.DataIndexXmlNamespace);
            return namespaces;
        }
    }
}
