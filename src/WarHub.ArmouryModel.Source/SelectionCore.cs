using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("selection")]
    public sealed partial class SelectionCore : SelectionParentBaseCore
    {
        [XmlAttribute("number")]
        public int Number { get; }

        [XmlAttribute("type")]
        public SelectionEntryKind Type { get; }

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; }

        [XmlArray("categories")]
        public ImmutableArray<CategoryCore> Categories { get; }
    }
}
