using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source.XmlFormat
{
    public readonly struct RootElementInfo
    {
        public RootElementInfo(RootElement element)
        {
            Element = element;
        }

        public RootElement Element { get; }

        public string Namespace => NamespaceFromElement[Element];

        public string XmlElementName => XmlNames[Element];

        public SourceKind SourceKind => SourceKindFromElement[Element];

        public Type SerializationProxyType => SerializationProxyTypeFromElement[Element];

        public Type BuilderType => BuilderTypeFromElement[Element];

        public BattleScribeVersion CurrentVersion
        {
            get
            {
                switch (Element)
                {
                    case RootElement.Catalogue:
                    case RootElement.GameSystem:
                    case RootElement.Roster:
                    case RootElement.DataIndex:
                        return BattleScribeVersion.V2_02;
                    default:
                        return null;
                }
            }
        }

        public static ImmutableSortedSet<RootElement> AllElements { get; }
            = ImmutableSortedSet.Create(
                RootElement.Catalogue,
                RootElement.GameSystem,
                RootElement.Roster,
                RootElement.DataIndex);

        public override string ToString() => Element.ToString();

        internal static ImmutableDictionary<RootElement, SourceKind> SourceKindFromElement { get; }
            = new Dictionary<RootElement, SourceKind>
            {
                [RootElement.Catalogue] = SourceKind.Catalogue,
                [RootElement.DataIndex] = SourceKind.DataIndex,
                [RootElement.GameSystem] = SourceKind.Gamesystem,
                [RootElement.Roster] = SourceKind.Roster,
            }.ToImmutableDictionary();

        internal static ImmutableDictionary<RootElement, Type> SerializationProxyTypeFromElement { get; }
            = new Dictionary<RootElement, Type>
            {
                [RootElement.Catalogue] = typeof(CatalogueCore.FastSerializationProxy),
                [RootElement.DataIndex] = typeof(DataIndexCore.FastSerializationProxy),
                [RootElement.GameSystem] = typeof(GamesystemCore.FastSerializationProxy),
                [RootElement.Roster] = typeof(RosterCore.FastSerializationProxy),
            }.ToImmutableDictionary();

        internal static ImmutableDictionary<RootElement, Type> BuilderTypeFromElement { get; }
            = new Dictionary<RootElement, Type>
            {
                [RootElement.Catalogue] = typeof(CatalogueCore.Builder),
                [RootElement.DataIndex] = typeof(DataIndexCore.Builder),
                [RootElement.GameSystem] = typeof(GamesystemCore.Builder),
                [RootElement.Roster] = typeof(RosterCore.Builder),
            }.ToImmutableDictionary();

        internal static ImmutableDictionary<SourceKind, RootElement> ElementFromSourceKind { get; }
            = SourceKindFromElement
            .ToImmutableDictionary(x => x.Value, x => x.Key);

        internal static ImmutableDictionary<RootElement, string> NamespaceFromElement { get; }
            = new Dictionary<RootElement, string>
            {
                [RootElement.Catalogue] = Namespaces.CatalogueXmlns,
                [RootElement.DataIndex] = Namespaces.DataIndexXmlns,
                [RootElement.GameSystem] = Namespaces.GamesystemXmlns,
                [RootElement.Roster] = Namespaces.RosterXmlns,
            }.ToImmutableDictionary();

        internal static ImmutableDictionary<string, RootElement> RootElementFromXmlName { get; }
            = new Dictionary<string, RootElement>
            {
                [RootElementNames.Catalogue] = RootElement.Catalogue,
                [RootElementNames.DataIndex] = RootElement.DataIndex,
                [RootElementNames.GameSystem] = RootElement.GameSystem,
                [RootElementNames.Roster] = RootElement.Roster,
            }.ToImmutableDictionary();

        internal static ImmutableDictionary<RootElement, string> XmlNames { get; }
            = RootElementFromXmlName
            .ToImmutableDictionary(x => x.Value, x => x.Key);
    }
}
