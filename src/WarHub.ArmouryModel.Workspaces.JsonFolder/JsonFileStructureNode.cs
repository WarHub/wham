using System.IO;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    public abstract class JsonFileStructureNode
    {
        public JsonFileStructureNode(FileSystemInfo fileSystemInfo, JsonWorkspace workspace)
        {
            Workspace = workspace;
            FileSystemInfo = fileSystemInfo;
        }

        protected FileSystemInfo FileSystemInfo { get; }

        public string Path => FileSystemInfo.FullName;

        public string Name => FileSystemInfo.Name;

        public JsonWorkspace Workspace { get; }

        public abstract void Accept(JsonFileStructureVisitor visitor);

        public abstract TResult Accept<TResult>(JsonFileStructureVisitor<TResult> visitor);
    }
}
