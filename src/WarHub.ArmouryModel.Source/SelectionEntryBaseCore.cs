using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class SelectionEntryBaseCore : EntryBaseCore
    {
        [XmlAttribute("collective")]
        public bool Collective { get; }

        [XmlArray("constraints", Order = 0)]
        public ImmutableArray<ConstraintCore> Constraints { get; }

        [XmlArray("selectionEntries", Order = 1)]
        public ImmutableArray<SelectionEntryCore> SelectionEntries { get; }

        [XmlArray("selectionEntryGroups", Order = 2)]
        public ImmutableArray<SelectionEntryGroupCore> SelectionEntryGroups { get; }

        [XmlArray("entryLinks", Order = 3)]
        public ImmutableArray<EntryLinkCore> EntryLinks { get; }
    }
}
