using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record CatalogueBaseCore : CommentableCore
    {
        [XmlAttribute("id")]
        public abstract string? Id { get; init; }

        [XmlAttribute("name")]
        public abstract string? Name { get; init; }

        [XmlAttribute("revision")]
        public abstract int Revision { get; init; }

        [XmlAttribute("battleScribeVersion")]
        public abstract string? BattleScribeVersion { get; init; }

        [XmlAttribute("authorName")]
        public abstract string? AuthorName { get; init; }

        [XmlAttribute("authorContact")]
        public abstract string? AuthorContact { get; init; }

        [XmlAttribute("authorUrl")]
        public abstract string? AuthorUrl { get; init; }

        [XmlElement("readme")]
        public abstract string? Readme { get; init; }

        [XmlArray("publications")]
        public abstract ImmutableArray<PublicationCore> Publications { get; init; }

        [XmlArray("costTypes")]
        public abstract ImmutableArray<CostTypeCore> CostTypes { get; init; }

        [XmlArray("profileTypes")]
        public abstract ImmutableArray<ProfileTypeCore> ProfileTypes { get; init; }

        [XmlArray("categoryEntries")]
        public abstract ImmutableArray<CategoryEntryCore> CategoryEntries { get; init; }

        [XmlArray("forceEntries")]
        public abstract ImmutableArray<ForceEntryCore> ForceEntries { get; init; }

        [XmlArray("selectionEntries")]
        public abstract ImmutableArray<SelectionEntryCore> SelectionEntries { get; init; }

        [XmlArray("entryLinks")]
        public abstract ImmutableArray<EntryLinkCore> EntryLinks { get; init; }

        [XmlArray("rules")]
        public abstract ImmutableArray<RuleCore> Rules { get; init; }

        [XmlArray("infoLinks")]
        public abstract ImmutableArray<InfoLinkCore> InfoLinks { get; init; }

        [XmlArray("sharedSelectionEntries")]
        public abstract ImmutableArray<SelectionEntryCore> SharedSelectionEntries { get; init; }

        [XmlArray("sharedSelectionEntryGroups")]
        public abstract ImmutableArray<SelectionEntryGroupCore> SharedSelectionEntryGroups { get; init; }

        [XmlArray("sharedRules")]
        public abstract ImmutableArray<RuleCore> SharedRules { get; init; }

        [XmlArray("sharedProfiles")]
        public abstract ImmutableArray<ProfileCore> SharedProfiles { get; init; }

        [XmlArray("sharedInfoGroups")]
        public abstract ImmutableArray<InfoGroupCore> SharedInfoGroups { get; init; }
    }
}
