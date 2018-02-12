using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("categoryLink")]
    public partial class CategoryLinkCore : LinkBaseCore
    {
        [XmlAttribute("primary")]
        public bool IsPrimary { get; }

        [XmlArray("constraints")]
        public ImmutableArray<ConstraintCore> Constraints { get; }
    }
}
