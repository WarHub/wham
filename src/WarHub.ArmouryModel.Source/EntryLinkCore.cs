using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("entryLink")]
    public sealed partial record EntryLinkCore : SelectionEntryBaseCore
    {
        [XmlAttribute("targetId")]
        public string? TargetId { get; init; }

        [XmlAttribute("type")]
        public EntryLinkKind Type { get; init; }

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; init; } = ImmutableArray<CostCore>.Empty;
    }
}
