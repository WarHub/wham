namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public partial record DerivedTwiceWithNewPropsCore : AbstractDerivedWithNewPropCore
    {
        public RecursiveContainerCore Container { get; init; } = RecursiveContainerCore.Empty;
    }
}
