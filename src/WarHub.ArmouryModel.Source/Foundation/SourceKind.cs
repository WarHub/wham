namespace WarHub.ArmouryModel.Source
{
    public enum SourceKind
    {
        Unknown,

        // types
        CostType,
        CharacteristicType,
        ProfileType,

        // entries
        SelectionEntry,
        SelectionEntryGroup,
        CategoryEntry,
        ForceEntry,
        DataIndexEntry,
        DataIndexRepositoryUrl,
        Metadata,

        // selectors + modifiers
        Condition,
        ConditionGroup,
        Constraint,
        Repeat,
        Modifier,

        // infos
        Cost,
        Characteristic,
        Profile,
        Rule,

        // links
        CategoryLink,
        EntryLink,
        InfoLink,

        // roster elements 
        CostLimit,
        Category,
        Force,
        Selection,

        // top elements
        Catalogue,
        Gamesystem,
        Roster,
        DataIndex,
        Datablob,
    }
}
