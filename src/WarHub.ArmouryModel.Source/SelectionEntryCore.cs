using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("selectionEntry")]
    public partial class SelectionEntryCore : SelectionEntryBaseCore
    {
        [XmlAttribute("categoryEntryId")]
        public string CategoryEntryId { get; }

        [XmlAttribute("type")]
        public SelectionEntryKind Type { get; }

        [XmlArray("costs", Order = 0)]
        public ImmutableArray<CostCore> Costs { get; }
    }
}
