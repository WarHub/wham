using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record SelectionEntryBaseCore : ContainerEntryBaseCore
    {
        [XmlAttribute("collective")]
        public abstract bool Collective { get; init; }

        [XmlAttribute("import")]
        public abstract bool Exported { get; init; }

        [XmlArray("categoryLinks")]
        public abstract ImmutableArray<CategoryLinkCore> CategoryLinks { get; init; }

        [XmlArray("selectionEntries")]
        public abstract ImmutableArray<SelectionEntryCore> SelectionEntries { get; init; }

        [XmlArray("selectionEntryGroups")]
        public abstract ImmutableArray<SelectionEntryGroupCore> SelectionEntryGroups { get; init; }

        [XmlArray("entryLinks")]
        public abstract ImmutableArray<EntryLinkCore> EntryLinks { get; init; }
    }
}
