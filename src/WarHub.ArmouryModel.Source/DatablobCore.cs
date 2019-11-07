using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// Represents a blob of collections of any other possible entity.
    /// This is a package that can contain anything, and additionally
    /// allows specification of metadata.
    /// </summary>
    [WhamNodeCore]
    public partial class DatablobCore
    {
        public MetadataCore Meta { get; }

        public ImmutableArray<CatalogueCore> Catalogues { get; }

        public ImmutableArray<CategoryCore> Categories { get; }

        public ImmutableArray<CategoryEntryCore> CategoryEntries { get; }

        public ImmutableArray<CategoryLinkCore> CategoryLinks { get; }

        public ImmutableArray<CharacteristicCore> Characteristics { get; }

        public ImmutableArray<CharacteristicTypeCore> CharacteristicTypes { get; }

        public ImmutableArray<ConditionCore> Conditions { get; }

        public ImmutableArray<ConditionGroupCore> ConditionGroups { get; }

        public ImmutableArray<ConstraintCore> Constraints { get; }

        public ImmutableArray<CostCore> Costs { get; }

        public ImmutableArray<CostLimitCore> CostLimits { get; }

        public ImmutableArray<CostTypeCore> CostTypes { get; }

        public ImmutableArray<DatablobCore> Datablobs { get; }

        public ImmutableArray<DataIndexCore> DataIndexes { get; }

        public ImmutableArray<DataIndexEntryCore> DataIndexEntries { get; }

        public ImmutableArray<EntryLinkCore> EntryLinks { get; }

        public ImmutableArray<ForceCore> Forces { get; }

        public ImmutableArray<ForceEntryCore> ForceEntries { get; }

        public ImmutableArray<GamesystemCore> Gamesystems { get; }

        public ImmutableArray<InfoGroupCore> InfoGroups { get; }

        public ImmutableArray<InfoLinkCore> InfoLinks { get; }

        public ImmutableArray<ModifierCore> Modifiers { get; }

        public ImmutableArray<ModifierGroupCore> ModifierGroups { get; }

        public ImmutableArray<ProfileCore> Profiles { get; }

        public ImmutableArray<ProfileTypeCore> ProfileTypes { get; }

        public ImmutableArray<PublicationCore> Publications { get; }

        public ImmutableArray<RepeatCore> Repeats { get; }

        public ImmutableArray<RosterCore> Rosters { get; }

        public ImmutableArray<RuleCore> Rules { get; }

        public ImmutableArray<SelectionCore> Selections { get; }

        public ImmutableArray<SelectionEntryCore> SelectionEntries { get; }

        public ImmutableArray<SelectionEntryGroupCore> SelectionEntryGroups { get; }
    }
}
