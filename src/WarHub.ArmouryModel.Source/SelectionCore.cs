using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("selection")]
    public partial class SelectionCore : RosterElementBaseCore
    {
        [XmlAttribute("number")]
        public int Number { get; }

        [XmlAttribute("type")]
        public SelectionEntryKind Type { get; }

        [XmlArray("selections")]
        public ImmutableArray<SelectionCore> Selections { get; }

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; }

        [XmlArray("categories")]
        public ImmutableArray<CategoryCore> Categories { get; }
    }
}
