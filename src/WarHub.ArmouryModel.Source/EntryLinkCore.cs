using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("entryLink")]
    public partial class EntryLinkCore : LinkBaseCore
    {
        [XmlAttribute("type")]
        public EntryLinkKind Type { get; }

        [XmlAttribute("categoryEntryId")]
        public string CategoryEntryId { get; }

        [XmlArray("constraints", Order = 0)]
        public ImmutableArray<ConstraintCore> Constraints { get; }
    }
}
