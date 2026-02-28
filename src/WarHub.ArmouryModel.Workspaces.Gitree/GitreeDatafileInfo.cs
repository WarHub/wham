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
        public SourceKind DataKind => GetData().Kind;

        private WeakReference<SourceNode> WeakData { get; } = new WeakReference<SourceNode>(null);

        public SourceNode GetData(CancellationToken cancellationToken = default)
        {
            if (WeakData.TryGetTarget(out var cached))
            {
                return cached;
            }
            return ReadAndCacheData();
        }

        public Task<SourceNode> GetDataAsync(CancellationToken cancellationToken = default)
        {
            if (WeakData.TryGetTarget(out var cached))
            {
                return Task.FromResult(cached);
            }
            return Task.FromResult(ReadAndCacheData());
        }

        public string GetStorageName() => new FileInfo(Filepath).Directory.Name;

        public bool TryGetData(out SourceNode node) => WeakData.TryGetTarget(out node);

        private SourceNode ReadAndCacheData()
        {
            var rootItem = new GitreeReader().ReadItemFolder(RootDocument.Parent);
            var node = new GitreeToSourceNodeConverter().ParseNode(rootItem);
            WeakData.SetTarget(node);
            return node;
        }
    }
}
