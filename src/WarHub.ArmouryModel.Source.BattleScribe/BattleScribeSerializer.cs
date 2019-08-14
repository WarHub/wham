using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.BattleScribe.Utilities;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    /// <summary>
    /// Caches namespaces and serializers required for serialization and deserialization,
    /// and provides methods for performing those operations in strongly typed fashion.
    /// </summary>
    public class BattleScribeXmlSerializer
    {
        private readonly Dictionary<RootElement, XmlSerializerNamespaces> namespaces
            = new Dictionary<RootElement, XmlSerializerNamespaces>();

        private readonly Dictionary<RootElement, XmlSerializer> serializers
            = new Dictionary<RootElement, XmlSerializer>();

        private readonly Dictionary<RootElement, XmlSerializer> deserializers
            = new Dictionary<RootElement, XmlSerializer>();

        public GamesystemNode DeserializeGamesystem(Stream stream)
            => Deserialize<GamesystemCore.Builder>(stream, RootElement.GameSystem).ToImmutable().ToNode();

        public CatalogueNode DeserializeCatalogue(Stream stream)
            => Deserialize<CatalogueCore.Builder>(stream, RootElement.Catalogue).ToImmutable().ToNode();

        public RosterNode DeserializeRoster(Stream stream)
            => Deserialize<RosterCore.Builder>(stream, RootElement.Roster).ToImmutable().ToNode();

        public DataIndexNode DeserializeDataIndex(Stream stream)
            => Deserialize<DataIndexCore.Builder>(stream, RootElement.DataIndex).ToImmutable().ToNode();

        public CatalogueNode DeserializeCatalogue(Func<XmlSerializer, object> deserialization)
        {
            var builder = (CatalogueCore.Builder)deserialization(GetDeserializer(RootElement.Catalogue));
            return builder.ToImmutable().ToNode();
        }

        public GamesystemNode DeserializeGamesystem(Func<XmlSerializer, object> deserialization)
        {
            var builder = (GamesystemCore.Builder)deserialization(GetDeserializer(RootElement.GameSystem));
            return builder.ToImmutable().ToNode();
        }

        public RosterNode DeserializeRoster(Func<XmlSerializer, object> deserialization)
        {
            var builder = (RosterCore.Builder)deserialization(GetDeserializer(RootElement.Roster));
            return builder.ToImmutable().ToNode();
        }

        public DataIndexNode DeserializeDataIndex(Func<XmlSerializer, object> deserialization)
        {
            var builder = (DataIndexCore.Builder)deserialization(GetDeserializer(RootElement.DataIndex));
            return builder.ToImmutable().ToNode();
        }

        public void SerializeGamesystem(GamesystemNode node, Stream stream)
            => Serialize(stream, node.GetSerializationProxy(), node.Kind.ToRootElement());

        public void SerializeCatalogue(CatalogueNode node, Stream stream)
            => Serialize(stream, node.GetSerializationProxy(), node.Kind.ToRootElement());

        public void SerializeRoster(RosterNode node, Stream stream)
            => Serialize(stream, node.GetSerializationProxy(), node.Kind.ToRootElement());

        public void SerializeDataIndex(DataIndexNode node, Stream stream)
            => Serialize(stream, node.GetSerializationProxy(), node.Kind.ToRootElement());

        private void Serialize(Stream stream, object @object, RootElement rootElement)
        {
            var serializer = GetSerializer(rootElement);
            var namespaces = GetNamespaces(rootElement);
            using (var writer = BattleScribeConformantXmlWriter.Create(stream))
            {
                serializer.Serialize(writer, @object, namespaces);
            }
        }

        private T Deserialize<T>(Stream stream, RootElement rootElement)
        {
            var serializer = GetDeserializer(rootElement);
            return (T)serializer.Deserialize(stream);
        }

        private XmlSerializerNamespaces GetNamespaces(RootElement rootElement)
        {
            if (namespaces.TryGetValue(rootElement, out var cached))
            {
                return cached;
            }
            var created = CreateNamespaces(rootElement);
            namespaces[rootElement] = created;
            return created;
        }

        private XmlSerializer GetSerializer(RootElement rootElement)
        {
            if (serializers.TryGetValue(rootElement, out var cached))
            {
                return cached;
            }
            var created = CreateSerializer(rootElement);
            serializers[rootElement] = created;
            return created;
        }

        private XmlSerializer GetDeserializer(RootElement rootElement)
        {
            if (deserializers.TryGetValue(rootElement, out var cached))
            {
                return cached;
            }
            var created = CreateDeserializer(rootElement);
            deserializers[rootElement] = created;
            return created;
        }

        private static XmlSerializer CreateSerializer(RootElement rootElement)
            => new XmlSerializer(rootElement.Info().SerializationProxyType);

        private static XmlSerializer CreateDeserializer(RootElement rootElement)
            => new XmlSerializer(rootElement.Info().BuilderType);

        private static XmlSerializerNamespaces CreateNamespaces(RootElement rootElement)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", rootElement.Info().Namespace);
            return namespaces;
        }
    }
}
