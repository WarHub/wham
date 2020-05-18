using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class CatalogueBaseCore : CommentableCore
    {
        [XmlAttribute("id")]
        public string? Id { get; }

        [XmlAttribute("name")]
        public string? Name { get; }

        [XmlAttribute("revision")]
        public int Revision { get; }

        [XmlAttribute("battleScribeVersion")]
        public string? BattleScribeVersion { get; }

        [XmlAttribute("authorName")]
        public string? AuthorName { get; }

        [XmlAttribute("authorContact")]
        public string? AuthorContact { get; }

        [XmlAttribute("authorUrl")]
        public string? AuthorUrl { get; }

        [XmlArray("publications")]
        public ImmutableArray<PublicationCore> Publications { get; }

        [XmlArray("costTypes")]
        public ImmutableArray<CostTypeCore> CostTypes { get; }

        [XmlArray("profileTypes")]
        public ImmutableArray<ProfileTypeCore> ProfileTypes { get; }

        [XmlArray("categoryEntries")]
        public ImmutableArray<CategoryEntryCore> CategoryEntries { get; }

        [XmlArray("forceEntries")]
        public ImmutableArray<ForceEntryCore> ForceEntries { get; }

        [XmlArray("selectionEntries")]
        public ImmutableArray<SelectionEntryCore> SelectionEntries { get; }

        [XmlArray("entryLinks")]
        public ImmutableArray<EntryLinkCore> EntryLinks { get; }

        [XmlArray("rules")]
        public ImmutableArray<RuleCore> Rules { get; }

        [XmlArray("infoLinks")]
        public ImmutableArray<InfoLinkCore> InfoLinks { get; }

        [XmlArray("sharedSelectionEntries")]
        public ImmutableArray<SelectionEntryCore> SharedSelectionEntries { get; }

        [XmlArray("sharedSelectionEntryGroups")]
        public ImmutableArray<SelectionEntryGroupCore> SharedSelectionEntryGroups { get; }

        [XmlArray("sharedRules")]
        public ImmutableArray<RuleCore> SharedRules { get; }

        [XmlArray("sharedProfiles")]
        public ImmutableArray<ProfileCore> SharedProfiles { get; }

        [XmlArray("sharedInfoGroups")]
        public ImmutableArray<InfoGroupCore> SharedInfoGroups { get; }
    }
}
