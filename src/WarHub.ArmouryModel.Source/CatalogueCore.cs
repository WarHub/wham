using System.Collections.Immutable;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(RootElementNames.Catalogue, Namespace = Namespaces.CatalogueXmlns, IsNullable = false)]
    public sealed partial class CatalogueCore : CatalogueBaseCore
    {
        [XmlAttribute("library")]
        public bool IsLibrary { get; }

        [XmlAttribute("gameSystemId")]
        public string GamesystemId { get; }

        [XmlAttribute("gameSystemRevision")]
        public int GamesystemRevision { get; }

        [XmlArray("catalogueLinks")]
        public ImmutableArray<CatalogueLinkCore> CatalogueLinks { get; }
    }
}
