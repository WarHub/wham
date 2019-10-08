using System.IO;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    internal abstract class GitreeStorageBaseNode
    {
        protected GitreeStorageBaseNode(FileSystemInfo fileSystemInfo, GitreeStorageFolderNode parent, GitreeWorkspace workspace)
        {
            FileSystemInfo = fileSystemInfo;
            Parent = parent;
            Workspace = workspace;
        }

        protected FileSystemInfo FileSystemInfo { get; }

        public GitreeStorageFolderNode Parent { get; }

        public string Path => FileSystemInfo.FullName;

        public string Name => FileSystemInfo.Name;

        public GitreeWorkspace Workspace { get; }
    }
}
