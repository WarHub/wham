using System;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public static class DatafileInfo
    {
        public static IDatafileInfo<TNode> Create<TNode>(string filepath, TNode? node) where TNode : SourceNode
        {
            if (node is null)
            {
                return (IDatafileInfo<TNode>)(IDatafileInfo<SourceNode>)new UnknownTypeDatafileInfo(filepath);
            }
            var visitor = new Visitor(filepath);
            return visitor.Visit(node) as IDatafileInfo<TNode> ?? throw new NotSupportedException("Unsupported root node.");
        }

        private sealed class Visitor : SourceVisitor<IDatafileInfo<SourceNode>>
        {
            public Visitor(string filepath)
            {
                Filepath = filepath;
            }

            private string Filepath { get; }

            private DatafileInfo<TNode> Create<TNode>(TNode node) where TNode : SourceNode
            {
                return new DatafileInfo<TNode>(Filepath, node);
            }

            public override IDatafileInfo<SourceNode> DefaultVisit(SourceNode node) => Create(node);

            public override IDatafileInfo<SourceNode> VisitCatalogue(CatalogueNode node) => Create(node);

            public override IDatafileInfo<SourceNode> VisitGamesystem(GamesystemNode node) => Create(node);

            public override IDatafileInfo<SourceNode> VisitRoster(RosterNode node) => Create(node);

            public override IDatafileInfo<SourceNode> VisitDataIndex(DataIndexNode node) => Create(node);
        }
    }
}
