using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class CatalogueBaseCore
    {
        [XmlAttribute("id")]
        public string Id { get; }

        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("book")]
        public string Book { get; }

        [XmlAttribute("page")]
        public string Page { get; }

        [XmlAttribute("revision")]
        public int Revision { get; }

        [XmlAttribute("battleScribeVersion")]
        public string BattleScribeVersion { get; }

        [XmlAttribute("authorName")]
        public string AuthorName { get; }

        [XmlAttribute("authorContact")]
        public string AuthorContact { get; }

        [XmlAttribute("authorUrl")]
        public string AuthorUrl { get; }

        [XmlArray("profiles", Order = 0)]
        public ImmutableArray<ProfileCore> Profiles { get; }

        [XmlArray("rules", Order = 1)]
        public ImmutableArray<RuleCore> Rules { get; }

        [XmlArray("infoLinks", Order = 2)]
        public ImmutableArray<InfoLinkCore> InfoLinks { get; }

        [XmlArray("costTypes", Order = 3)]
        public ImmutableArray<CostTypeCore> CostTypes { get; }

        [XmlArray("profileTypes", Order = 4)]
        public ImmutableArray<ProfileTypeCore> ProfileTypes { get; }

        [XmlArray("forceEntries", Order = 5)]
        public ImmutableArray<ForceEntryCore> ForceEntries { get; }

        [XmlArray("selectionEntries", Order = 6)]
        public ImmutableArray<SelectionEntryCore> SelectionEntries { get; }

        [XmlArray("entryLinks", Order = 7)]
        public ImmutableArray<EntryLinkCore> EntryLinks { get; }

        [XmlArray("sharedSelectionEntries", Order = 8)]
        public ImmutableArray<SelectionEntryCore> SharedSelectionEntries { get; }

        [XmlArray("sharedSelectionEntryGroups", Order = 9)]
        public ImmutableArray<SelectionEntryGroupCore> SharedSelectionEntryGroups { get; }

        [XmlArray("sharedRules", Order = 10)]
        public ImmutableArray<RuleCore> SharedRules { get; }

        [XmlArray("sharedProfiles", Order = 11)]
        public ImmutableArray<ProfileCore> SharedProfiles { get; }
    }
}
