using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("selection")]
    public sealed partial record SelectionCore : SelectionParentBaseCore
    {
        [XmlAttribute("number")]
        public int Number { get; init; }

        [XmlAttribute("type")]
        public SelectionEntryKind Type { get; init; }

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; init; } = ImmutableArray<CostCore>.Empty;

        [XmlArray("categories")]
        public ImmutableArray<CategoryCore> Categories { get; init; } = ImmutableArray<CategoryCore>.Empty;
    }
}
