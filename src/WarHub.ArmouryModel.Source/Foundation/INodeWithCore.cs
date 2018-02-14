namespace WarHub.ArmouryModel.Source
{
    public interface INodeWithCore<TCore>
    {
        TCore Core { get; }
    }
}
