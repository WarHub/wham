using System;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    internal class JsonDatafileInfo : IDatafileInfo
    {
        public JsonDatafileInfo(JsonDocument rootDocument)
        {
            RootDocument = rootDocument;
            LazyData = new Lazy<SourceNode>(ReadData);
        }

        public string Filepath => RootDocument.Path;

        public SourceNode Data => LazyData.Value;

        public JsonDocument RootDocument { get; }

        private Lazy<SourceNode> LazyData { get; }

        private SourceNode ReadData()
        {
            var rootItem = new JsonTreeReader().ReadItemFolder(RootDocument.Parent);
            var node = new JsonTreeToSourceNodeConverter().ParseItem(rootItem);
            return node;
        }
    }
}
