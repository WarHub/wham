using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    public class JsonFolder : JsonFileStructureNode
    {
        private readonly DirectoryInfo directory;

        public JsonFolder(DirectoryInfo directory, JsonFolder parent, JsonWorkspace workspace)
            : base(directory, parent, workspace)
        {
            this.directory = directory;
            Documents = new Lazy<ImmutableArray<JsonDocument>>(CreateDocuments);
            Folders = new Lazy<ImmutableArray<JsonFolder>>(CreateFolders);
        }

        private Lazy<ImmutableArray<JsonDocument>> Documents { get; }

        private Lazy<ImmutableArray<JsonFolder>> Folders { get; }

        public ImmutableArray<JsonDocument> GetDocuments() => Documents.Value;

        public ImmutableArray<JsonFolder> GetFolders() => Folders.Value;

        private ImmutableArray<JsonDocument> CreateDocuments()
        {
            return directory
                .EnumerateFiles("*.json")
                .Select(file => new JsonDocument(file, this, Workspace))
                .ToImmutableArray();
        }

        private ImmutableArray<JsonFolder> CreateFolders()
        {
            return directory
                .EnumerateDirectories()
                .Select(dir => new JsonFolder(dir, this, Workspace))
                .ToImmutableArray();
        }
    }
}
