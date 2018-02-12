namespace WarHub.ArmouryModel.Source
{
    internal interface INodeWithCore<TCore>
    {
        TCore Core { get; }
    }
}
