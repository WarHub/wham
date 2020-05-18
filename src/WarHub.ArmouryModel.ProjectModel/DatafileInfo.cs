using System;
using System.IO;
using Amadevus.RecordGenerator;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    [Record]
    public sealed partial class DatafileInfo<TData> : IDatafileInfo<TData>
        where TData : SourceNode
    {
        // TODO internal ctor

        public string Filepath { get; }

        public TData Data { get; }

        public SourceKind DataKind => Data.Kind;

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);

        TData? IDatafileInfo<TData>.GetData() => Data;

        SourceNode? IDatafileInfo.GetData() => Data;
    }

    public static class DatafileInfo
    {
        public static IDatafileInfo<TData> Create<TData>(string filepath, TData? node) where TData : SourceNode
        {
            if (node is null)
            {
                return (IDatafileInfo<TData>)(IDatafileInfo<SourceNode>)new UnknownTypeDatafileInfo(filepath);
            }
            var visitor = new Visitor(filepath);
            return visitor.Visit(node) as IDatafileInfo<TData> ?? throw new NotSupportedException("Unsupported root node.");
        }

        private class Visitor : SourceVisitor<IDatafileInfo<SourceNode>>
        {
            public Visitor(string filepath)
            {
                Filepath = filepath;
            }

            private string Filepath { get; }

            private DatafileInfo<TData> Create<TData>(TData node) where TData : SourceNode
            {
                return new DatafileInfo<TData>(Filepath, node);
            }

            public override IDatafileInfo<SourceNode> DefaultVisit(SourceNode node) => Create(node);

            public override IDatafileInfo<SourceNode> VisitCatalogue(CatalogueNode node) => Create(node);

            public override IDatafileInfo<SourceNode> VisitGamesystem(GamesystemNode node) => Create(node);

            public override IDatafileInfo<SourceNode> VisitRoster(RosterNode node) => Create(node);

            public override IDatafileInfo<SourceNode> VisitDataIndex(DataIndexNode node) => Create(node);
        }
    }
}
