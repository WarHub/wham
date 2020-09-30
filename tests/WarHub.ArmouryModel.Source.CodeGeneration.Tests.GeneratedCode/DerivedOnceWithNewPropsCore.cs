namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public partial record DerivedOnceWithNewPropsCore : AbstractBaseCore
    {
        public bool Flag { get; init; }
    }
}
