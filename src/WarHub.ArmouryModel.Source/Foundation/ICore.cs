namespace WarHub.ArmouryModel.Source
{
    public interface ICore<out TNode> where TNode : SourceNode
    {
        TNode ToNode(SourceNode? parent = null);
    }
}
