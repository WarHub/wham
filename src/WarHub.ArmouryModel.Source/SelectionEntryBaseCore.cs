using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record SelectionEntryBaseCore : ContainerEntryBaseCore
    {
        [XmlAttribute("collective")]
        public bool Collective { get; init; }

        [XmlAttribute("import")]
        public bool Exported { get; init; }

        [XmlArray("categoryLinks")]
        public ImmutableArray<CategoryLinkCore> CategoryLinks { get; init; } = ImmutableArray<CategoryLinkCore>.Empty;

        [XmlArray("selectionEntries")]
        public ImmutableArray<SelectionEntryCore> SelectionEntries { get; init; } = ImmutableArray<SelectionEntryCore>.Empty;

        [XmlArray("selectionEntryGroups")]
        public ImmutableArray<SelectionEntryGroupCore> SelectionEntryGroups { get; init; } = ImmutableArray<SelectionEntryGroupCore>.Empty;

        [XmlArray("entryLinks")]
        public ImmutableArray<EntryLinkCore> EntryLinks { get; init; } = ImmutableArray<EntryLinkCore>.Empty;
    }
}
