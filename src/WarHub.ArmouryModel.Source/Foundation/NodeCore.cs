namespace WarHub.ArmouryModel.Source
{
    public abstract class NodeCore : ICore<SourceNode>
    {
        public SourceNode ToNode(SourceNode? parent = null)
        {
            return ToNodeCore(parent);
        }

        protected abstract SourceNode ToNodeCore(SourceNode? parent);
    }
}
