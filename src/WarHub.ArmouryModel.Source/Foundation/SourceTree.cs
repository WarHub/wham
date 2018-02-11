namespace WarHub.ArmouryModel.Source
{
    public class SourceTree
    {
    }

    public enum SourceKind
    {
        // types
        CostType,
        CharacteristicType,
        ProfileType,

        // entries
        SelectionEntry,
        SelectionEntryGroup,
        CategoryEntry,
        ForceEntry,

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

        // top elements
        Catalogue,
        Gamesystem,
        Roster,

        // roster elements 
        CostLimit,
        Category,
        Force,
        Selection,
    }
}
