namespace WarHub.ArmouryModel.Source
{
    public interface IBuilder<T>
    {
        T ToImmutable();
    }
}