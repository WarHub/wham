using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    [DebuggerDisplay("{" + nameof(file) + "}")]
    internal class GitreeStorageFileNode : GitreeStorageBaseNode
    {
        private readonly FileInfo file;

        public GitreeStorageFileNode(FileInfo file, GitreeStorageFolderNode parent, GitreeWorkspace workspace)
            : base(file, parent, workspace)
        {
            this.file = file;
        }

        private WeakReference<DatablobNode> WeakRoot { get; }
            = new WeakReference<DatablobNode>(null);

        /// <summary>
        /// Gets the root node of the document. May cause deserialization.
        /// </summary>
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
            using var fileStream = File.OpenText(Path);
            using var jsonReader = new JsonTextReader(fileStream);
            return Workspace.Serializer.Deserialize<DatablobCore>(jsonReader).ToNode();
        }
    }
}
