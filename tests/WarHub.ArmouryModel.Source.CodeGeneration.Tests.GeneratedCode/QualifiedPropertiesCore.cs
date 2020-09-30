namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public partial record QualifiedPropertiesCore
    {
        public System.DateTime Date { get; init; }

        public System.Collections.Immutable.ImmutableArray<ItemCore> Items { get; init; } = System.Collections.Immutable.ImmutableArray<ItemCore>.Empty;
    }
}
