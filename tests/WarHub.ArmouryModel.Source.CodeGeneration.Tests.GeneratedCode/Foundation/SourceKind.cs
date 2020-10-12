namespace WarHub.ArmouryModel.Source
{
    public enum SourceKind
    {
        Unknown,

        Gamesystem, GamesystemList,

        Catalogue, CatalogueList,

        Class1, Class1List,

        Container, ContainerList,

        DerivedOnce, DerivedOnceList,

        DerivedOnceWithNewProps, DerivedOnceWithNewPropsList,

        DerivedTwice, DerivedTwiceList,

        DerivedTwiceWithNewProps, DerivedTwiceWithNewPropsList,

        Item, ItemList,

        NotOnlyAutoGetter, NotOnlyAutoGetterList,

        QualifiedProperties, QualifiedPropertiesList,

        RecursiveContainer, RecursiveContainerList,

        RootContainer, RootContainerList,

        TestBuilderPartial, TestBuilderPartialList,
    }
}
