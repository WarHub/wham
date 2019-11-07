namespace WarHub.ArmouryModel.Source
{
    internal interface IContainerProvider<out TNode> where TNode : SourceNode
    {
        IContainer<TNode> Container { get; }
    }
}
