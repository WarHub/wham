using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    internal class GitreeDatafileInfo : IDatafileInfo
    {
        public GitreeDatafileInfo(GitreeStorageFileNode rootDocument)
        {
            RootDocument = rootDocument;
        }

        public string Filepath => RootDocument.Path;

        public GitreeStorageFileNode RootDocument { get; }

        // TODO should be optimized to read data type from single "root" file
        // TODO shouldn't block
        public SourceKind DataKind => GetData().Kind;

        private WeakReference<SourceNode> WeakData { get; } = new WeakReference<SourceNode>(null);

        public SourceNode GetData(CancellationToken cancellationToken = default)
        {
            if (WeakData.TryGetTarget(out var cached))
            {
                return cached;
            }
            return GetDataAsync(cancellationToken).GetAwaiter().GetResult();
        }

        public async Task<SourceNode> GetDataAsync(CancellationToken cancellationToken = default)
        {
            if (WeakData.TryGetTarget(out var cached))
            {
                return cached;
            }
            var data = await ReadDataAsync();
            WeakData.SetTarget(data);
            return data;
        }

        public string GetStorageName() => new FileInfo(Filepath).Directory.Name;

        public bool TryGetData(out SourceNode node) => WeakData.TryGetTarget(out node);

        private Task<SourceNode> ReadDataAsync()
        {
            var rootItem = new GitreeReader().ReadItemFolder(RootDocument.Parent);
            var node = new GitreeToSourceNodeConverter().ParseNode(rootItem);
            return Task.FromResult(node);
        }
    }
}
