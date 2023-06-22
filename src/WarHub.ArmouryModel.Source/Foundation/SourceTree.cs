using System.Threading;
using System.Threading.Tasks;

namespace WarHub.ArmouryModel.Source
{
    public abstract class SourceTree
    {
        public abstract string? FilePath { get; }

        public virtual bool TryGetRoot(out SourceNode root)
        {
            root = GetRoot();
            return true;
        }

        public abstract SourceNode GetRoot(CancellationToken cancellationToken = default);

        public abstract Task<SourceNode> GetRootAsync(CancellationToken cancellationToken = default);

        public abstract SourceTree WithRoot(SourceNode root);

        public abstract FileLinePositionSpan GetLineSpan(TextSpan span);

        public abstract Location GetLocation(TextSpan span);

        public static SourceTree CreateForRoot(SourceNode rootNode, string? filepath = null) =>
            new InMemoryTree(rootNode, filepath);

        protected SourceNode NodeForThisTree(SourceNode node) =>
            node.WithTree(this);

        private sealed class InMemoryTree : SourceTree
        {
            private readonly SourceNode root;
            private readonly string? filepath;

            public InMemoryTree(SourceNode root, string? filepath)
            {
                this.filepath = filepath;
                this.root = NodeForThisTree(root);
            }

            public override string? FilePath => filepath;

            public override FileLinePositionSpan GetLineSpan(TextSpan span)
            {
                return default; // TODO implement
            }

            public override Location GetLocation(TextSpan span)
            {
                return new SourceLocation(this, span);
            }

            public override SourceNode GetRoot(CancellationToken cancellationToken = default) => root;

            public override Task<SourceNode> GetRootAsync(CancellationToken cancellationToken = default) =>
                Task.FromResult(root);

            public override InMemoryTree WithRoot(SourceNode newRootNode) =>
                new(newRootNode, FilePath);
        }
    }
}
