using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("force")]
    public sealed partial class ForceCore : RosterElementBaseCore
    {
        [XmlAttribute("catalogueId")]
        public string CatalogueId { get; }

        [XmlAttribute("catalogueRevision")]
        public int CatalogueRevision { get; }

        [XmlAttribute("catalogueName")]
        public string? CatalogueName { get; }

        [XmlArray("selections")]
        public ImmutableArray<SelectionCore> Selections { get; }

        [XmlArray("publications")]
        public ImmutableArray<PublicationCore> Publications { get; }

        [XmlArray("categories")]
        public ImmutableArray<CategoryCore> Categories { get; }

        [XmlArray("forces")]
        public ImmutableArray<ForceCore> Forces { get; }
    }
}
