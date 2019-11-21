using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.BattleScribe.Utilities;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    /// <summary>
    /// Caches namespaces and serializers required for serialization and deserialization,
    /// and provides methods for performing those operations in strongly typed fashion.
    /// </summary>
    public sealed class BattleScribeXmlSerializer
    {
        private static Lazy<BattleScribeXmlSerializer> LazyInstance { get; }
            = new Lazy<BattleScribeXmlSerializer>();

        /// <summary>
        /// Gets a static instance of this class.
        /// </summary>
        public static BattleScribeXmlSerializer Instance => LazyInstance.Value;

        private class ElementCache
        {
            public XmlSerializer Serializer;
            public XmlSerializer Deserializer;
            public XmlSerializerNamespaces Namespaces;
        }

        private readonly ImmutableDictionary<RootElement, ElementCache> elementCacheDictionary
            = new Dictionary<RootElement, ElementCache>
            {
                [RootElement.Catalogue] = new ElementCache(),
                [RootElement.DataIndex] = new ElementCache(),
                [RootElement.GameSystem] = new ElementCache(),
                [RootElement.Roster] = new ElementCache(),
            }.ToImmutableDictionary();

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
            return rootElement switch
            {
                RootElement.Catalogue => DeserializeCatalogue(deserialization),
                RootElement.GameSystem => DeserializeGamesystem(deserialization),
                RootElement.Roster => DeserializeRoster(deserialization),
                RootElement.DataIndex => DeserializeDataIndex(deserialization),
                _ => throw new ArgumentException(
                        $"Deserialization is not supported for this {nameof(RootElement)}.",
                        nameof(rootElement)),
            };
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
            using var bsWriter = BattleScribeConformantXmlWriter.Create(writer);
            serializer.Serialize(bsWriter, @object, namespaces);
        }

        private T Deserialize<T>(Func<XmlSerializer, object> deserialization, RootElement rootElement)
        {
            var serializer = GetDeserializer(rootElement);
            return (T)deserialization(serializer);
        }

        private XmlSerializerNamespaces GetNamespaces(RootElement rootElement)
        {
            ref var cached = ref elementCacheDictionary[rootElement].Namespaces;
            if (cached is { } existing)
            {
                return existing;
            }
            var created = CreateNamespaces(rootElement);
            return Interlocked.CompareExchange(ref cached, created, null) is null
                ? created : cached;

            static XmlSerializerNamespaces CreateNamespaces(RootElement rootElement)
            {
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", rootElement.Info().Namespace);
                return namespaces;
            }
        }

        private XmlSerializer GetSerializer(RootElement rootElement)
        {
            ref var cached = ref elementCacheDictionary[rootElement].Serializer;
            if (cached is { } existing)
            {
                return existing;
            }
            var created = CreateSerializer(rootElement);
            return Interlocked.CompareExchange(ref cached, created, null) is null
                ? created : cached;

            static XmlSerializer CreateSerializer(RootElement rootElement)
                => new XmlSerializer(rootElement.Info().SerializationProxyType);
        }

        private XmlSerializer GetDeserializer(RootElement rootElement)
        {
            ref var cached = ref elementCacheDictionary[rootElement].Deserializer;
            if (cached is { } existing)
            {
                return existing;
            }
            var created = CreateDeserializer(rootElement);
            return Interlocked.CompareExchange(ref cached, created, null) is null
                ? created : cached;

            static XmlSerializer CreateDeserializer(RootElement rootElement)
                => new XmlSerializer(rootElement.Info().BuilderType);
        }
    }
}
