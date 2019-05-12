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

        [XmlArray("profiles")]
        public ImmutableArray<ProfileCore> Profiles { get; }

        [XmlArray("rules")]
        public ImmutableArray<RuleCore> Rules { get; }

        [XmlArray("infoGroups")]
        public ImmutableArray<InfoGroupCore> InfoGroups { get; }

        [XmlArray("infoLinks")]
        public ImmutableArray<InfoLinkCore> InfoLinks { get; }

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
