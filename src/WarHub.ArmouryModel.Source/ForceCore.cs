using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("force")]
    public sealed partial record ForceCore : SelectionParentBaseCore
    {
        [XmlAttribute("catalogueId")]
        public string? CatalogueId { get; init; }

        [XmlAttribute("catalogueRevision")]
        public int CatalogueRevision { get; init; }

        [XmlAttribute("catalogueName")]
        public string? CatalogueName { get; init; }

        [XmlArray("publications")]
        public ImmutableArray<PublicationCore> Publications { get; init; } = ImmutableArray<PublicationCore>.Empty;

        [XmlArray("categories")]
        public ImmutableArray<CategoryCore> Categories { get; init; } = ImmutableArray<CategoryCore>.Empty;

        [XmlArray("forces")]
        public ImmutableArray<ForceCore> Forces { get; init; } = ImmutableArray<ForceCore>.Empty;
    }
}
