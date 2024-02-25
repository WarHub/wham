using System.Threading;
using System.Threading.Tasks;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public sealed record DatafileInfo<TNode>(string Filepath, TNode Node) : IDatafileInfo<TNode>
        where TNode : SourceNode
    {
        private Task<SourceNode>? lazyTask;

        public SourceKind DataKind => Node.Kind;

        public Task<SourceNode> GetDataAsync(CancellationToken cancellationToken = default) =>
            lazyTask ??= Task.FromResult<SourceNode>(Node);

        public SourceNode GetData(CancellationToken cancellationToken = default) => Node;

        public bool TryGetData(out SourceNode node)
        {
            node = Node;
            return true;
        }
    }
}
