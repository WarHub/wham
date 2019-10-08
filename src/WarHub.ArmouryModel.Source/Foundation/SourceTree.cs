namespace WarHub.ArmouryModel.Source
{
    public abstract class SourceTree
    {
        public abstract string FilePath { get; }

        public bool TryGetRoot(out SourceNode root)
        {
            return TryGetRootCore(out root);
        }

        protected abstract bool TryGetRootCore(out SourceNode root);

        public SourceNode GetRoot()
        {
            return GetRootCore();
        }

        protected abstract SourceNode GetRootCore();

        public SourceTree WithRoot(SourceNode root)
        {
            var newNode = root.Core.ToNode();
            var newTree = WithRootCore(newNode);
            newNode.Tree = newTree;
            return newTree;
        }

        internal abstract SourceTree WithRootCore(SourceNode root);
    }
}
