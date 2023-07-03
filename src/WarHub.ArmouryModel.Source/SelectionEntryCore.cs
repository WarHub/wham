using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("selectionEntry")]
    public sealed partial record SelectionEntryCore : SelectionEntryBaseCore
    {
        /// <inheritdoc />
        [XmlAttribute("id")]
        public override string? Id { get; init; }

        /// <inheritdoc />
        [XmlAttribute("name")]
        public override string? Name { get; init; }

        /// <inheritdoc />
        [XmlAttribute("publicationId")]
        public override string? PublicationId { get; init; }

        /// <inheritdoc />
        [XmlAttribute("page")]
        public override string? Page { get; init; }

        /// <inheritdoc />
        [XmlAttribute("hidden")]
        public override bool Hidden { get; init; }

        [XmlAttribute("collective")]
        public override bool Collective { get; init; }

        [XmlAttribute("import")]
        public override bool Exported { get; init; }

        [XmlAttribute("type")]
        public SelectionEntryKind Type { get; init; }

        /// <inheritdoc />
        [XmlElement("comment")]
        public override string? Comment { get; init; }

        /// <inheritdoc />
        [XmlArray("modifiers")]
        public override ImmutableArray<ModifierCore> Modifiers { get; init; } = ImmutableArray<ModifierCore>.Empty;

        /// <inheritdoc />
        [XmlArray("modifierGroups")]
        public override ImmutableArray<ModifierGroupCore> ModifierGroups { get; init; } = ImmutableArray<ModifierGroupCore>.Empty;

        /// <inheritdoc />
        [XmlArray("constraints")]
        public override ImmutableArray<ConstraintCore> Constraints { get; init; } = ImmutableArray<ConstraintCore>.Empty;

        /// <inheritdoc />
        [XmlArray("profiles")]
        public override ImmutableArray<ProfileCore> Profiles { get; init; } = ImmutableArray<ProfileCore>.Empty;

        /// <inheritdoc />
        [XmlArray("rules")]
        public override ImmutableArray<RuleCore> Rules { get; init; } = ImmutableArray<RuleCore>.Empty;

        /// <inheritdoc />
        [XmlArray("infoGroups")]
        public override ImmutableArray<InfoGroupCore> InfoGroups { get; init; } = ImmutableArray<InfoGroupCore>.Empty;

        /// <inheritdoc />
        [XmlArray("infoLinks")]
        public override ImmutableArray<InfoLinkCore> InfoLinks { get; init; } = ImmutableArray<InfoLinkCore>.Empty;

        /// <inheritdoc />
        [XmlArray("categoryLinks")]
        public override ImmutableArray<CategoryLinkCore> CategoryLinks { get; init; } = ImmutableArray<CategoryLinkCore>.Empty;

        /// <inheritdoc />
        [XmlArray("selectionEntries")]
        public override ImmutableArray<SelectionEntryCore> SelectionEntries { get; init; } = ImmutableArray<SelectionEntryCore>.Empty;

        /// <inheritdoc />
        [XmlArray("selectionEntryGroups")]
        public override ImmutableArray<SelectionEntryGroupCore> SelectionEntryGroups { get; init; } = ImmutableArray<SelectionEntryGroupCore>.Empty;

        /// <inheritdoc />
        [XmlArray("entryLinks")]
        public override ImmutableArray<EntryLinkCore> EntryLinks { get; init; } = ImmutableArray<EntryLinkCore>.Empty;

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; init; } = ImmutableArray<CostCore>.Empty;
    }
}
