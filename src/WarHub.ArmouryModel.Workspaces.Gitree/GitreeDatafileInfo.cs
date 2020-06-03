﻿using System;
using System.IO;
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
        public SourceKind DataKind => GetDataAsync().Result.Kind;

        private WeakReference<SourceNode> WeakData { get; } = new WeakReference<SourceNode>(null);

        public async Task<SourceNode> GetDataAsync()
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

        private Task<SourceNode> ReadDataAsync()
        {
            var rootItem = new GitreeReader().ReadItemFolder(RootDocument.Parent);
            var node = new GitreeToSourceNodeConverter().ParseNode(rootItem);
            return Task.FromResult(node);
        }
    }
}
