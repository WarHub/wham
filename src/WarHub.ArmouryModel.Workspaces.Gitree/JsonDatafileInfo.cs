using System;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    internal class JsonDatafileInfo : IDatafileInfo
    {
        public JsonDatafileInfo(JsonDocument rootDocument)
        {
            RootDocument = rootDocument;
        }

        public string Filepath => RootDocument.Path;

        public JsonDocument RootDocument { get; }

        // TODO should be optimized to read data type from single "root" file
        public SourceKind DataKind => GetData().Kind;


        private WeakReference<SourceNode> WeakData { get; } = new WeakReference<SourceNode>(null);

        public SourceNode GetData()
        {
            if (WeakData.TryGetTarget(out var cached))
            {
                return cached;
            }
            var data = ReadData();
            WeakData.SetTarget(data);
            return data;
        }

        private SourceNode ReadData()
        {
            var rootItem = new JsonTreeReader().ReadItemFolder(RootDocument.Parent);
            var node = new JsonTreeToSourceNodeConverter().ParseItem(rootItem);
            return node;
        }
    }
}
