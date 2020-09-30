namespace WarHub.ArmouryModel.Source
{
    public abstract class NodeCore : ICore<SourceNode>
    {
        public abstract SourceNode ToNode(SourceNode? parent = null);
    }
}
