namespace WarHub.ArmouryModel.Source
{
    public enum SourceKind
    {
        Unknown,

        // types
        CostType, CostTypeList,
        CharacteristicType, CharacteristicTypeList,
        ProfileType, ProfileTypeList,

        // entries
        SelectionEntry, SelectionEntryList,
        SelectionEntryGroup, SelectionEntryGroupList,
        CategoryEntry, CategoryEntryList,
        ForceEntry, ForceEntryList,
        DataIndexEntry, DataIndexEntryList,
        DataIndexRepositoryUrl, DataIndexRepositoryUrlList,
        Metadata, MetadataList,

        // selectors + modifiers
        Condition, ConditionList,
        ConditionGroup, ConditionGroupList,
        Constraint, ConstraintList,
        Repeat, RepeatList,
        Modifier, ModifierList,

        // infos
        Cost, CostList,
        Characteristic, CharacteristicList,
        Profile, ProfileList, 
        Publication, PublicationList,
        Rule, RuleList,

        // links
        CategoryLink, CategoryLinkList,
        EntryLink, EntryLinkList,
        InfoLink, InfoLinkList,
        CatalogueLink, CatalogueLinkList,

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
