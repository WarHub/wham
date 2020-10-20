using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source.XmlFormat
{
    public readonly struct RootElementInfo : IEquatable<RootElementInfo>
    {
        public RootElementInfo(RootElement element)
        {
            Element = element;
        }

        public RootElement Element { get; }

        public string Namespace => NamespaceFromElement[Element];

        public string XmlElementName => XmlNames[Element];

        public SourceKind SourceKind => SourceKindFromElement[Element];

        public XmlSerializer Serializer => XmlSerializerFromElement[Element];

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
                        return BattleScribeVersion.V2x03;
                    default:
                        throw new NotSupportedException($"This {nameof(RootElement)} value is not known.");
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

        public override bool Equals(object? obj)
        {
            return obj is RootElementInfo info && Equals(info);
        }

        public bool Equals(RootElementInfo other)
        {
            return Element == other.Element;
        }

        public override int GetHashCode()
        {
            return -703426257 + Element.GetHashCode();
        }

        public static bool operator ==(RootElementInfo left, RootElementInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RootElementInfo left, RootElementInfo right)
        {
            return !(left == right);
        }

        internal static ImmutableDictionary<RootElement, SourceKind> SourceKindFromElement { get; }
            = new Dictionary<RootElement, SourceKind>
            {
                [RootElement.Catalogue] = SourceKind.Catalogue,
                [RootElement.DataIndex] = SourceKind.DataIndex,
                [RootElement.GameSystem] = SourceKind.Gamesystem,
                [RootElement.Roster] = SourceKind.Roster,
            }.ToImmutableDictionary();

        internal static ImmutableDictionary<RootElement, XmlSerializer> XmlSerializerFromElement { get; }
            = new Dictionary<RootElement, XmlSerializer>
            {
                [RootElement.Catalogue] = new CatalogueCoreXmlSerializer(),
                [RootElement.DataIndex] = new DataIndexCoreXmlSerializer(),
                [RootElement.GameSystem] = new GamesystemCoreXmlSerializer(),
                [RootElement.Roster] = new RosterCoreXmlSerializer(),
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
