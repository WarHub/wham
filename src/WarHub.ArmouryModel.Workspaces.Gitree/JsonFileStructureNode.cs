using System.IO;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    public abstract class JsonFileStructureNode
    {
        public JsonFileStructureNode(FileSystemInfo fileSystemInfo, JsonFolder parent, JsonWorkspace workspace)
        {
            FileSystemInfo = fileSystemInfo;
            Parent = parent;
            Workspace = workspace;
        }

        protected FileSystemInfo FileSystemInfo { get; }

        public JsonFolder Parent { get; }

        public string Path => FileSystemInfo.FullName;

        public string Name => FileSystemInfo.Name;

        public JsonWorkspace Workspace { get; }
    }
}
