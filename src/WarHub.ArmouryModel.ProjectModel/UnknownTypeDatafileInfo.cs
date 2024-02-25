using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public record UnknownTypeDatafileInfo(string Filepath) : IDatafileInfo, IDatafileInfo<SourceNode>
    {
        public SourceKind DataKind => SourceKind.Unknown;

        public SourceNode Node => throw new InvalidOperationException("There is no data for Unknown datafile.");

        public SourceNode GetData(CancellationToken cancellationToken = default) => Node;

        public Task<SourceNode> GetDataAsync(CancellationToken cancellationToken = default)
        {
            var node = Node;
            return Task.FromResult(node);
        }

        public bool TryGetData([NotNullWhen(true)] out SourceNode? node)
        {
            node = null;
            return false;
        }
    }
}
