namespace WarHub.ArmouryModel.Source
{
    public interface INodeWithCore<out TCore>
    {
        TCore Core { get; }
    }
}
