namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public partial class QualifiedPropertiesCore
    {
        public System.DateTime Date { get; }

        public System.Collections.Immutable.ImmutableArray<ItemCore> Items { get; }
    }
}
