using System;
using System.IO;
using Newtonsoft.Json;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    public class JsonDocument : JsonFileStructureNode
    {
        private readonly FileInfo file;

        public JsonDocument(FileInfo file, JsonWorkspace workspace) : base(file, workspace)
        {
            this.file = file;
        }

        private WeakReference<DatablobNode> WeakRoot { get; } = new WeakReference<DatablobNode>(null);

        public override void Accept(JsonFileStructureVisitor visitor)
        {
            visitor.VisitDocument(this);
        }

        public override TResult Accept<TResult>(JsonFileStructureVisitor<TResult> visitor)
        {
            return visitor.VisitDocument(this);
        }

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
                return Workspace.Serializer.Deserialize<DatablobCore>(jsonReader).ToNode();
            }
        }
    }
}
