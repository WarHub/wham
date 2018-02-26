using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public class JsonWorkspace
    {
        public JsonWorkspace(IEnumerable<FileInfo> files)
        {
            Documents =
                files
                .Select(file => new JsonDocument(file, this))
                .ToImmutableArray();
            Serializer = new JsonSerializer
            {
                Converters =
                {
                }
            };
        }

        public ImmutableArray<JsonDocument> Documents { get; }

        /// <summary>
        /// Creates workspace from directory by indexing it's contents (and all subdirectories
        /// if specified using <paramref name="searchOption"/>) for files with .json extension.
        /// </summary>
        /// <param name="path">Directory path to search in.</param>
        /// <param name="searchOption">Specify to search all sub-directories.</param>
        /// <returns>Workspace created from the directory with all files with .json extension.</returns>
        public static JsonWorkspace CreateFromDirectory(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var dirInfo = new DirectoryInfo(path);
            var files = dirInfo.EnumerateFiles("*.json", searchOption);
            return new JsonWorkspace(files);
        }

        internal JsonSerializer Serializer { get; }
    }

    public class JsonDocument
    {
        private readonly FileInfo file;

        public JsonDocument(FileInfo file, JsonWorkspace workspace)
        {
            this.file = file;
            Workspace = workspace;
            Path = file.FullName;
            Name = file.Name;
        }

        /// <summary>
        /// Gets the filepath of this document.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the filename without file extension.
        /// </summary>
        public string Name { get; }

        public JsonWorkspace Workspace { get; }

        private WeakReference<DatablobNode> WeakRoot { get; } = new WeakReference<DatablobNode>(null);

        /// <summary>
        /// Gets the root node of the document. May cause deserialization.
        /// </summary>
        /// <returns></returns>
        public DatablobNode GetRoot()
        {
            return GetRootCore();
        }

        private DatablobNode GetRootCore()
        {
            if (WeakRoot.TryGetTarget(out var root))
            {
                return root;
            }
            root = LoadRoot();
            WeakRoot.SetTarget(root);
            return root;
        }

        private DatablobNode LoadRoot()
        {
            using (var fileStream = File.OpenText(Path))
            using (var jsonReader = new JsonTextReader(fileStream))
            {
                return new JsonSerializer() { }.Deserialize<DatablobCore>(jsonReader).ToNode();
            }
        }
    }
}
