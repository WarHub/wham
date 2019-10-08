namespace WarHub.ArmouryModel.Source
{
    public interface IBuildable<T, TBuilder> where TBuilder : IBuilder<T>
    {
        TBuilder ToBuilder();
    }
}
