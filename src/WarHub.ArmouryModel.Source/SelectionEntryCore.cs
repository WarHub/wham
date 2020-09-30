using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("selectionEntry")]
    public sealed partial record SelectionEntryCore : SelectionEntryBaseCore
    {
        [XmlAttribute("type")]
        public SelectionEntryKind Type { get; init; }

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; init; } = ImmutableArray<CostCore>.Empty;
    }
}
