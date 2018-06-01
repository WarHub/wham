using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    internal class GitreeStorageFolderNode : GitreeStorageBaseNode
    {
        private readonly DirectoryInfo directory;

        public GitreeStorageFolderNode(DirectoryInfo directory, GitreeStorageFolderNode parent, GitreeWorkspace workspace)
            : base(directory, parent, workspace)
        {
            this.directory = directory;
            DocumentsLazy = new Lazy<ImmutableArray<GitreeStorageFileNode>>(CreateDocuments);
            FoldersLazy = new Lazy<ImmutableArray<GitreeStorageFolderNode>>(CreateFolders);
        }

        private Lazy<ImmutableArray<GitreeStorageFileNode>> DocumentsLazy { get; }

        private Lazy<ImmutableArray<GitreeStorageFolderNode>> FoldersLazy { get; }

        public ImmutableArray<GitreeStorageFileNode> GetDocuments() => DocumentsLazy.Value;

        public ImmutableArray<GitreeStorageFolderNode> GetFolders() => FoldersLazy.Value;

        private ImmutableArray<GitreeStorageFileNode> CreateDocuments()
        {
            return directory
                .EnumerateFiles("*.json")
                .Select(file => new GitreeStorageFileNode(file, this, Workspace))
                .ToImmutableArray();
        }

        private ImmutableArray<GitreeStorageFolderNode> CreateFolders()
        {
            return directory
                .EnumerateDirectories()
                .Select(dir => new GitreeStorageFolderNode(dir, this, Workspace))
                .ToImmutableArray();
        }
    }
}
