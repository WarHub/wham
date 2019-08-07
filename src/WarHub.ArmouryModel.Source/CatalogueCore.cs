using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(
        XmlInformation.RootElementNames.Catalogue,
        Namespace = XmlInformation.Namespaces.CatalogueXmlns,
        IsNullable = false)]
    public partial class CatalogueCore : CatalogueBaseCore
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
