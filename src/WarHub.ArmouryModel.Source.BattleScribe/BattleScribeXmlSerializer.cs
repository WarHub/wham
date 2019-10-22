using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.BattleScribe.Utilities;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    /// <summary>
    /// Caches namespaces and serializers required for serialization and deserialization,
    /// and provides methods for performing those operations in strongly typed fashion.
    /// </summary>
    internal class BattleScribeXmlSerializer
    {
        private static Lazy<BattleScribeXmlSerializer> LazyInstance { get; }
            = new Lazy<BattleScribeXmlSerializer>();

        public static BattleScribeXmlSerializer Instance => LazyInstance.Value;

        private readonly Dictionary<RootElement, XmlSerializerNamespaces> namespaces
            = new Dictionary<RootElement, XmlSerializerNamespaces>();

        private readonly Dictionary<RootElement, XmlSerializer> serializers
            = new Dictionary<RootElement, XmlSerializer>();

        private readonly Dictionary<RootElement, XmlSerializer> deserializers
            = new Dictionary<RootElement, XmlSerializer>();

        public CatalogueNode DeserializeCatalogue(Func<XmlSerializer, object> deserialization)
            => Deserialize<CatalogueCore.Builder>(deserialization, RootElement.Catalogue)
            .ToImmutable().ToNode();

        public GamesystemNode DeserializeGamesystem(Func<XmlSerializer, object> deserialization)
            => Deserialize<GamesystemCore.Builder>(deserialization, RootElement.GameSystem)
            .ToImmutable().ToNode();

        public RosterNode DeserializeRoster(Func<XmlSerializer, object> deserialization)
            => Deserialize<RosterCore.Builder>(deserialization, RootElement.Roster)
            .ToImmutable().ToNode();

        public DataIndexNode DeserializeDataIndex(Func<XmlSerializer, object> deserialization)
            => Deserialize<DataIndexCore.Builder>(deserialization, RootElement.DataIndex)
            .ToImmutable().ToNode();

        public SourceNode Deserialize(
            Func<XmlSerializer, object> deserialization,
            RootElement rootElement)
        {
            switch (rootElement)
            {
                case RootElement.Catalogue:
                    return DeserializeCatalogue(deserialization);
                case RootElement.GameSystem:
                    return DeserializeGamesystem(deserialization);
                case RootElement.Roster:
                    return DeserializeRoster(deserialization);
                case RootElement.DataIndex:
                    return DeserializeDataIndex(deserialization);
                default:
                    throw new ArgumentException(
                        $"Deserialization is not supported for this {nameof(RootElement)}.",
                        nameof(rootElement));
            }
        }

        public void SerializeGamesystem(GamesystemNode node, TextWriter writer)
            => Serialize(writer, node.GetSerializationProxy(), node.Kind.ToRootElement());

        public void SerializeCatalogue(CatalogueNode node, TextWriter writer)
            => Serialize(writer, node.GetSerializationProxy(), node.Kind.ToRootElement());

        public void SerializeRoster(RosterNode node, TextWriter writer)
            => Serialize(writer, node.GetSerializationProxy(), node.Kind.ToRootElement());

        public void SerializeDataIndex(DataIndexNode node, TextWriter writer)
            => Serialize(writer, node.GetSerializationProxy(), node.Kind.ToRootElement());

        public void Serialize(SourceNode node, TextWriter writer)
        {
            switch (node.Kind)
            {
                case SourceKind.Gamesystem:
                    SerializeGamesystem((GamesystemNode)node, writer);
                    return;
                case SourceKind.Catalogue:
                    SerializeCatalogue((CatalogueNode)node, writer);
                    return;
                case SourceKind.Roster:
                    SerializeRoster((RosterNode)node, writer);
                    return;
                case SourceKind.DataIndex:
                    SerializeDataIndex((DataIndexNode)node, writer);
                    return;
                default:
                    throw new ArgumentException(
                        $"{nameof(node)} type's ({node?.GetType()}) serialization is not supported.",
                        nameof(node));
            }
        }

        private void Serialize(TextWriter writer, object @object, RootElement rootElement)
        {
            var serializer = GetSerializer(rootElement);
            var namespaces = GetNamespaces(rootElement);
            using (var bsWriter = BattleScribeConformantXmlWriter.Create(writer))
            {
                serializer.Serialize(bsWriter, @object, namespaces);
            }
        }

        private T Deserialize<T>(Func<XmlSerializer, object> deserialization, RootElement rootElement)
        {
            var serializer = GetDeserializer(rootElement);
            return (T)deserialization(serializer);
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
