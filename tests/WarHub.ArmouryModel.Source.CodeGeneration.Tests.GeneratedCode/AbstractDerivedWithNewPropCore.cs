namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record AbstractDerivedWithNewPropCore : AbstractBaseCore
    {
        public string? DerivedNewProp { get; init; }
    }
}
