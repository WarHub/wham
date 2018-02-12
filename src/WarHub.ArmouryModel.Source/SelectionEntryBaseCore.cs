using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class SelectionEntryBaseCore : EntryBaseCore
    {
        [XmlAttribute("collective")]
        public bool Collective { get; }

        [XmlArray("constraints")]
        public ImmutableArray<ConstraintCore> Constraints { get; }

        [XmlArray("categoryLinks")]
        public ImmutableArray<CategoryLinkCore> CategoryLinks { get; }

        [XmlArray("selectionEntries")]
        public ImmutableArray<SelectionEntryCore> SelectionEntries { get; }

        [XmlArray("selectionEntryGroups")]
        public ImmutableArray<SelectionEntryGroupCore> SelectionEntryGroups { get; }

        [XmlArray("entryLinks")]
        public ImmutableArray<EntryLinkCore> EntryLinks { get; }
    }
}
