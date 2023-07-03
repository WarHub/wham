using System.Collections.Immutable;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(RootElementNames.GameSystem, Namespace = Namespaces.GamesystemXmlns, IsNullable = false)]
    public sealed partial record GamesystemCore : CatalogueBaseCore
    {
        /// <inheritdoc />
        [XmlAttribute("id")]
        public override string? Id { get; init; }

        /// <inheritdoc />
        [XmlAttribute("name")]
        public override string? Name { get; init; }

        /// <inheritdoc />
        [XmlAttribute("revision")]
        public override int Revision { get; init; }

        /// <inheritdoc />
        [XmlAttribute("battleScribeVersion")]
        public override string? BattleScribeVersion { get; init; }

        /// <inheritdoc />
        [XmlAttribute("authorName")]
        public override string? AuthorName { get; init; }

        /// <inheritdoc />
        [XmlAttribute("authorContact")]
        public override string? AuthorContact { get; init; }

        /// <inheritdoc />
        [XmlAttribute("authorUrl")]
        public override string? AuthorUrl { get; init; }

        /// <inheritdoc />
        [XmlElement("comment")]
        public override string? Comment { get; init; }

        /// <inheritdoc />
        [XmlElement("readme")]
        public override string? Readme { get; init; }

        /// <inheritdoc />
        [XmlArray("publications")]
        public override ImmutableArray<PublicationCore> Publications { get; init; } = ImmutableArray<PublicationCore>.Empty;

        /// <inheritdoc />
        [XmlArray("costTypes")]
        public override ImmutableArray<CostTypeCore> CostTypes { get; init; } = ImmutableArray<CostTypeCore>.Empty;

        /// <inheritdoc />
        [XmlArray("profileTypes")]
        public override ImmutableArray<ProfileTypeCore> ProfileTypes { get; init; } = ImmutableArray<ProfileTypeCore>.Empty;

        /// <inheritdoc />
        [XmlArray("categoryEntries")]
        public override ImmutableArray<CategoryEntryCore> CategoryEntries { get; init; } = ImmutableArray<CategoryEntryCore>.Empty;

        /// <inheritdoc />
        [XmlArray("forceEntries")]
        public override ImmutableArray<ForceEntryCore> ForceEntries { get; init; } = ImmutableArray<ForceEntryCore>.Empty;

        /// <inheritdoc />
        [XmlArray("selectionEntries")]
        public override ImmutableArray<SelectionEntryCore> SelectionEntries { get; init; } = ImmutableArray<SelectionEntryCore>.Empty;

        /// <inheritdoc />
        [XmlArray("entryLinks")]
        public override ImmutableArray<EntryLinkCore> EntryLinks { get; init; } = ImmutableArray<EntryLinkCore>.Empty;

        /// <inheritdoc />
        [XmlArray("rules")]
        public override ImmutableArray<RuleCore> Rules { get; init; } = ImmutableArray<RuleCore>.Empty;

        /// <inheritdoc />
        [XmlArray("infoLinks")]
        public override ImmutableArray<InfoLinkCore> InfoLinks { get; init; } = ImmutableArray<InfoLinkCore>.Empty;

        /// <inheritdoc />
        [XmlArray("sharedSelectionEntries")]
        public override ImmutableArray<SelectionEntryCore> SharedSelectionEntries { get; init; } = ImmutableArray<SelectionEntryCore>.Empty;

        /// <inheritdoc />
        [XmlArray("sharedSelectionEntryGroups")]
        public override ImmutableArray<SelectionEntryGroupCore> SharedSelectionEntryGroups { get; init; } = ImmutableArray<SelectionEntryGroupCore>.Empty;

        /// <inheritdoc />
        [XmlArray("sharedRules")]
        public override ImmutableArray<RuleCore> SharedRules { get; init; } = ImmutableArray<RuleCore>.Empty;

        /// <inheritdoc />
        [XmlArray("sharedProfiles")]
        public override ImmutableArray<ProfileCore> SharedProfiles { get; init; } = ImmutableArray<ProfileCore>.Empty;

        /// <inheritdoc />
        [XmlArray("sharedInfoGroups")]
        public override ImmutableArray<InfoGroupCore> SharedInfoGroups { get; init; } = ImmutableArray<InfoGroupCore>.Empty;
    }
}
