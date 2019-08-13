using System.Collections.Generic;
using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source.XmlFormat
{
    public readonly struct RootElementInfo
    {
        internal RootElementInfo(RootElement element)
        {
            Element = element;
        }

        public RootElement Element { get; }

        public string Namespace => NamespaceFromElement[Element];

        public string XmlElementName => XmlNames[Element];

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

        public override string ToString() => Element.ToString();

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
