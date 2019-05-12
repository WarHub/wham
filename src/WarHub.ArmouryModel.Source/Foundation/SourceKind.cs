namespace WarHub.ArmouryModel.Source
{
    public enum SourceKind
    {
        Unknown,

        // types
        CharacteristicType, CharacteristicTypeList,
        CostType, CostTypeList,
        ProfileType, ProfileTypeList,

        // entries
        CategoryEntry, CategoryEntryList,
        DataIndexEntry, DataIndexEntryList,
        DataIndexRepositoryUrl, DataIndexRepositoryUrlList,
        ForceEntry, ForceEntryList,
        Metadata, MetadataList,
        SelectionEntry, SelectionEntryList,
        SelectionEntryGroup, SelectionEntryGroupList,

        // selectors + modifiers
        Condition, ConditionList,
        ConditionGroup, ConditionGroupList,
        Constraint, ConstraintList,
        Modifier, ModifierList,
        ModifierGroup, ModifierGroupList,
        Repeat, RepeatList,

        // infos
        Characteristic, CharacteristicList,
        Cost, CostList,
        InfoGroup, InfoGroupList,
        Profile, ProfileList, 
        Publication, PublicationList,
        Rule, RuleList,

        // links
        CatalogueLink, CatalogueLinkList,
        CategoryLink, CategoryLinkList,
        EntryLink, EntryLinkList,
        InfoLink, InfoLinkList,

        // roster elements 
        CostLimit, CostLimitList,
        Category, CategoryList,
        Force, ForceList,
        Selection, SelectionList,

        // top elements
        Catalogue, CatalogueList,
        Gamesystem, GamesystemList,
        Roster, RosterList,
        DataIndex, DataIndexList,
        Datablob, DatablobList,
    }
}
