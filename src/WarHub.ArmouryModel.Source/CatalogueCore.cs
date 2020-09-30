using System.Collections.Immutable;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(RootElementNames.Catalogue, Namespace = Namespaces.CatalogueXmlns, IsNullable = false)]
    public sealed partial record CatalogueCore : CatalogueBaseCore
    {
        [XmlAttribute("library")]
        public bool IsLibrary { get; init; }

        [XmlAttribute("gameSystemId")]
        public string? GamesystemId { get; init; }

        [XmlAttribute("gameSystemRevision")]
        public int GamesystemRevision { get; init; }

        [XmlArray("catalogueLinks")]
        public ImmutableArray<CatalogueLinkCore> CatalogueLinks { get; init; } = ImmutableArray<CatalogueLinkCore>.Empty;
    }
}
