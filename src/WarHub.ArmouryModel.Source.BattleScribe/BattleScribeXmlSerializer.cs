using System;
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

        private sealed class ElementCache
        {
            public XmlSerializer? Serializer;
            public XmlSerializerNamespaces? Namespaces;
        }

        private readonly ImmutableDictionary<RootElement, ElementCache> elementCacheDictionary
            = new Dictionary<RootElement, ElementCache>
            {
                [RootElement.Catalogue] = new ElementCache(),
                [RootElement.DataIndex] = new ElementCache(),
                [RootElement.GameSystem] = new ElementCache(),
                [RootElement.Roster] = new ElementCache(),
            }.ToImmutableDictionary();

        public CatalogueNode? DeserializeCatalogue(Func<XmlSerializer, object?> deserialization)
            => Deserialize<CatalogueCore>(deserialization, RootElement.Catalogue)?.ToNode();

        public GamesystemNode? DeserializeGamesystem(Func<XmlSerializer, object?> deserialization)
            => Deserialize<GamesystemCore>(deserialization, RootElement.GameSystem)?.ToNode();

        public RosterNode? DeserializeRoster(Func<XmlSerializer, object?> deserialization)
            => Deserialize<RosterCore>(deserialization, RootElement.Roster)?.ToNode();

        public DataIndexNode? DeserializeDataIndex(Func<XmlSerializer, object?> deserialization)
            => Deserialize<DataIndexCore>(deserialization, RootElement.DataIndex)?.ToNode();

        public SourceNode? Deserialize(
            Func<XmlSerializer, object?> deserialization,
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

        public void Serialize(SourceNode node, TextWriter writer)
        {
            switch (node.Kind)
            {
                case SourceKind.Gamesystem:
                case SourceKind.Catalogue:
                case SourceKind.Roster:
                case SourceKind.DataIndex:
                    Serialize(writer, ((INodeWithCore<NodeCore>)node).Core, node.Kind.ToRootElement());
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

        private T? Deserialize<T>(Func<XmlSerializer, object?> deserialization, RootElement rootElement)
        {
            var serializer = GetSerializer(rootElement);
            return (T?)deserialization(serializer);
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
            var created = rootElement.Info().Serializer;
            return Interlocked.CompareExchange(ref cached, created, null) is null
                ? created : cached;
        }
    }
}
