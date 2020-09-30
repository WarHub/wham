namespace WarHub.ArmouryModel.Source
{
    public abstract record NodeCore : ICore<SourceNode>
    {
        public abstract SourceNode ToNode(SourceNode? parent = null);
    }
}
