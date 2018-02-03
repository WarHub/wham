using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("selection")]
    public partial class SelectionCore : RosterElementBaseCore
    {
        [XmlAttribute("entryGroupId")]
        public string EntryGroupId { get; }

        [XmlAttribute("book")]
        public string Book { get; }

        [XmlAttribute("page")]
        public string Page { get; }

        [XmlAttribute("number")]
        public int Number { get; }

        [XmlAttribute("type")]
        public SelectionEntryKind Type { get; }

        [XmlArray("selections", Order = 0)]
        public ImmutableArray<SelectionCore> Selections { get; }

        [XmlArray("costs", Order = 1)]
        public ImmutableArray<CostCore> Costs { get; }
    }
}
