using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record CatalogueBaseCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("revision")]
        public int Revision { get; init; }

        [XmlAttribute("battleScribeVersion")]
        public string? BattleScribeVersion { get; init; }

        [XmlAttribute("authorName")]
        public string? AuthorName { get; init; }

        [XmlAttribute("authorContact")]
        public string? AuthorContact { get; init; }

        [XmlAttribute("authorUrl")]
        public string? AuthorUrl { get; init; }

        [XmlElement("readme")]
        public string? Readme { get; init; }

        [XmlArray("publications")]
        public ImmutableArray<PublicationCore> Publications { get; init; } = ImmutableArray<PublicationCore>.Empty;

        [XmlArray("costTypes")]
        public ImmutableArray<CostTypeCore> CostTypes { get; init; } = ImmutableArray<CostTypeCore>.Empty;

        [XmlArray("profileTypes")]
        public ImmutableArray<ProfileTypeCore> ProfileTypes { get; init; } = ImmutableArray<ProfileTypeCore>.Empty;

        [XmlArray("categoryEntries")]
        public ImmutableArray<CategoryEntryCore> CategoryEntries { get; init; } = ImmutableArray<CategoryEntryCore>.Empty;

        [XmlArray("forceEntries")]
        public ImmutableArray<ForceEntryCore> ForceEntries { get; init; } = ImmutableArray<ForceEntryCore>.Empty;

        [XmlArray("selectionEntries")]
        public ImmutableArray<SelectionEntryCore> SelectionEntries { get; init; } = ImmutableArray<SelectionEntryCore>.Empty;

        [XmlArray("entryLinks")]
        public ImmutableArray<EntryLinkCore> EntryLinks { get; init; } = ImmutableArray<EntryLinkCore>.Empty;

        [XmlArray("rules")]
        public ImmutableArray<RuleCore> Rules { get; init; } = ImmutableArray<RuleCore>.Empty;

        [XmlArray("infoLinks")]
        public ImmutableArray<InfoLinkCore> InfoLinks { get; init; } = ImmutableArray<InfoLinkCore>.Empty;

        [XmlArray("sharedSelectionEntries")]
        public ImmutableArray<SelectionEntryCore> SharedSelectionEntries { get; init; } = ImmutableArray<SelectionEntryCore>.Empty;

        [XmlArray("sharedSelectionEntryGroups")]
        public ImmutableArray<SelectionEntryGroupCore> SharedSelectionEntryGroups { get; init; } = ImmutableArray<SelectionEntryGroupCore>.Empty;

        [XmlArray("sharedRules")]
        public ImmutableArray<RuleCore> SharedRules { get; init; } = ImmutableArray<RuleCore>.Empty;

        [XmlArray("sharedProfiles")]
        public ImmutableArray<ProfileCore> SharedProfiles { get; init; } = ImmutableArray<ProfileCore>.Empty;

        [XmlArray("sharedInfoGroups")]
        public ImmutableArray<InfoGroupCore> SharedInfoGroups { get; init; } = ImmutableArray<InfoGroupCore>.Empty;
    }
}
