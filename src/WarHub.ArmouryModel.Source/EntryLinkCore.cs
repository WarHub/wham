using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("entryLink")]
    public sealed partial class EntryLinkCore : SelectionEntryBaseCore
    {
        [XmlAttribute("targetId")]
        public string? TargetId { get; }

        [XmlAttribute("type")]
        public EntryLinkKind Type { get; }

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; }
    }
}
