using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// Represents a blob of collections of any other possible entity.
    /// This is a package that can contain anything, and additionally
    /// allows specification of metadata.
    /// </summary>
    [WhamNodeCore]
    public sealed partial record DatablobCore
    {
        public MetadataCore? Meta { get; init; }

        public ImmutableArray<CatalogueCore> Catalogues { get; init; } = ImmutableArray<CatalogueCore>.Empty;

        public ImmutableArray<CategoryCore> Categories { get; init; } = ImmutableArray<CategoryCore>.Empty;

        public ImmutableArray<CategoryEntryCore> CategoryEntries { get; init; } = ImmutableArray<CategoryEntryCore>.Empty;

        public ImmutableArray<CategoryLinkCore> CategoryLinks { get; init; } = ImmutableArray<CategoryLinkCore>.Empty;

        public ImmutableArray<CharacteristicCore> Characteristics { get; init; } = ImmutableArray<CharacteristicCore>.Empty;

        public ImmutableArray<CharacteristicTypeCore> CharacteristicTypes { get; init; } = ImmutableArray<CharacteristicTypeCore>.Empty;

        public ImmutableArray<ConditionCore> Conditions { get; init; } = ImmutableArray<ConditionCore>.Empty;

        public ImmutableArray<ConditionGroupCore> ConditionGroups { get; init; } = ImmutableArray<ConditionGroupCore>.Empty;

        public ImmutableArray<ConstraintCore> Constraints { get; init; } = ImmutableArray<ConstraintCore>.Empty;

        public ImmutableArray<CostCore> Costs { get; init; } = ImmutableArray<CostCore>.Empty;

        public ImmutableArray<CostLimitCore> CostLimits { get; init; } = ImmutableArray<CostLimitCore>.Empty;

        public ImmutableArray<CostTypeCore> CostTypes { get; init; } = ImmutableArray<CostTypeCore>.Empty;

        public ImmutableArray<DatablobCore> Datablobs { get; init; } = ImmutableArray<DatablobCore>.Empty;

        public ImmutableArray<DataIndexCore> DataIndexes { get; init; } = ImmutableArray<DataIndexCore>.Empty;

        public ImmutableArray<DataIndexEntryCore> DataIndexEntries { get; init; } = ImmutableArray<DataIndexEntryCore>.Empty;

        public ImmutableArray<EntryLinkCore> EntryLinks { get; init; } = ImmutableArray<EntryLinkCore>.Empty;

        public ImmutableArray<ForceCore> Forces { get; init; } = ImmutableArray<ForceCore>.Empty;

        public ImmutableArray<ForceEntryCore> ForceEntries { get; init; } = ImmutableArray<ForceEntryCore>.Empty;

        public ImmutableArray<GamesystemCore> Gamesystems { get; init; } = ImmutableArray<GamesystemCore>.Empty;

        public ImmutableArray<InfoGroupCore> InfoGroups { get; init; } = ImmutableArray<InfoGroupCore>.Empty;

        public ImmutableArray<InfoLinkCore> InfoLinks { get; init; } = ImmutableArray<InfoLinkCore>.Empty;

        public ImmutableArray<ModifierCore> Modifiers { get; init; } = ImmutableArray<ModifierCore>.Empty;

        public ImmutableArray<ModifierGroupCore> ModifierGroups { get; init; } = ImmutableArray<ModifierGroupCore>.Empty;

        public ImmutableArray<ProfileCore> Profiles { get; init; } = ImmutableArray<ProfileCore>.Empty;

        public ImmutableArray<ProfileTypeCore> ProfileTypes { get; init; } = ImmutableArray<ProfileTypeCore>.Empty;

        public ImmutableArray<PublicationCore> Publications { get; init; } = ImmutableArray<PublicationCore>.Empty;

        public ImmutableArray<RepeatCore> Repeats { get; init; } = ImmutableArray<RepeatCore>.Empty;

        public ImmutableArray<RosterCore> Rosters { get; init; } = ImmutableArray<RosterCore>.Empty;

        public ImmutableArray<RuleCore> Rules { get; init; } = ImmutableArray<RuleCore>.Empty;

        public ImmutableArray<SelectionCore> Selections { get; init; } = ImmutableArray<SelectionCore>.Empty;

        public ImmutableArray<SelectionEntryCore> SelectionEntries { get; init; } = ImmutableArray<SelectionEntryCore>.Empty;

        public ImmutableArray<SelectionEntryGroupCore> SelectionEntryGroups { get; init; } = ImmutableArray<SelectionEntryGroupCore>.Empty;
    }
}
