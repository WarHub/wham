using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("force")]
    public partial class ForceCore : RosterElementBaseCore
    {
        [XmlAttribute("catalogueId")]
        public string CatalogueId { get; }

        [XmlAttribute("catalogueRevision")]
        public int CatalogueRevision { get; }

        [XmlAttribute("catalogueName")]
        public string CatalogueName { get; }

        [XmlArray("categories", Order = 0)]
        public ImmutableArray<CategoryCore> Categories { get; }

        [XmlArray("forces", Order = 1)]
        public ImmutableArray<ForceCore> Forces { get; }
    }
}
