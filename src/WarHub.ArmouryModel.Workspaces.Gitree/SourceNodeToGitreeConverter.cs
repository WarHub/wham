using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using MoreLinq;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{

    public class SourceNodeToGitreeConverter : SourceVisitor<GitreeNode>
    {
        static SourceNodeToGitreeConverter()
        {
            FolderKinds =
                new[] 
                {
                    SourceKind.CatalogueList,
                    SourceKind.DatablobList,
                    SourceKind.DataIndexList,
                    SourceKind.ForceEntryList,
                    SourceKind.ForceList,
                    SourceKind.GamesystemList,
                    SourceKind.ProfileList,
                    SourceKind.ProfileTypeList,
                    SourceKind.RosterList,
                    SourceKind.RuleList,
                    SourceKind.SelectionEntryGroupList,
                    SourceKind.SelectionEntryList,
                    SourceKind.SelectionList
                }
                .ToImmutableHashSet();
        }

        /// <summary>
        /// Gets a set of source kinds that will be separated from the entity into child folders.
        /// </summary>
        public static ImmutableHashSet<SourceKind> FolderKinds { get; }

        private FolderKindDroppingRewriter FolderDropper { get; } = new FolderKindDroppingRewriter();

        private static DatablobNode EmptyBlob { get; }
            = NodeFactory.Datablob(NodeFactory.Metadata(null, null, null));

        public override GitreeNode DefaultVisit(SourceNode node)
        {
            var blob = BlobWith(node);
            return new GitreeNode(blob, node, IsLeaf: true, ImmutableArray<GitreeListNode>.Empty);
        }

        public override GitreeNode VisitCatalogue(CatalogueNode node)
        {
            var result = VisitCatalogueBase(node);
            return new GitreeNode(EmptyBlob.AddCatalogues(result.node), node, IsLeaf: false, result.folders);
        }

        public override GitreeNode VisitForce(ForceNode node)
        {
            var listFolders = CreateListFolders(node);
            var strippedNode = DropFolders(node);
            return new GitreeNode(EmptyBlob.AddForces(strippedNode), node, IsLeaf: false, listFolders);
        }

        public override GitreeNode VisitForceEntry(ForceEntryNode node)
        {
            var listFolders = CreateListFolders(node);
            var strippedNode = DropFolders(node);
            return new GitreeNode(EmptyBlob.AddForceEntries(strippedNode), node, IsLeaf: false, listFolders);
        }

        public override GitreeNode VisitGamesystem(GamesystemNode node)
        {
            var result = VisitCatalogueBase(node);
            return new GitreeNode(EmptyBlob.AddGamesystems(result.node), node, IsLeaf: false, result.folders);
        }

        public override GitreeNode VisitRoster(RosterNode node)
        {
            var listFolders = CreateListFolders(node);
            var strippedNode = DropFolders(node);
            return new GitreeNode(EmptyBlob.AddRosters(strippedNode), node, IsLeaf: false, listFolders);
        }

        public override GitreeNode VisitSelection(SelectionNode node)
        {
            var listFolders = CreateListFolders(node);
            var strippedNode = DropFolders(node);
            return new GitreeNode(EmptyBlob.AddSelections(strippedNode), node, IsLeaf: false, listFolders);
        }

        public override GitreeNode VisitSelectionEntry(SelectionEntryNode node)
        {
            var result = VisitSelectionEntryBase(node);
            return new GitreeNode(EmptyBlob.AddSelectionEntries(result.node), node, IsLeaf: false, result.folders);
        }

        public override GitreeNode VisitSelectionEntryGroup(SelectionEntryGroupNode node)
        {
            var result = VisitSelectionEntryBase(node);
            return new GitreeNode(EmptyBlob.AddSelectionEntryGroups(result.node), node, IsLeaf: false, result.folders);
        }

        private (T node, ImmutableArray<GitreeListNode> folders) VisitCatalogueBase<T>(T node)
            where T : CatalogueBaseNode
        {
            var listFolders = CreateListFolders(node);
            var strippedNode = DropFolders(node);
            return ((T)strippedNode, listFolders);
        }

        private (T node, ImmutableArray<GitreeListNode> folders) VisitSelectionEntryBase<T>(T node)
            where T : SelectionEntryBaseNode
        {
            var listFolders = CreateListFolders(node);
            var strippedNode = DropFolders(node);
            return ((T)strippedNode, listFolders);
        }

        private ImmutableArray<GitreeListNode> CreateListFolders<TNode>(TNode node)
            where TNode : SourceNode
        {
            // TODO select by common set of XyzListNode types that should be made into folders
            var listFolders = node
                .ChildrenInfos()
                .Where(info => FolderKinds.Contains(info.Node.Kind))
                .Select(CreateListFolder)
                .ToImmutableArray();
            return listFolders;
        }

        private GitreeListNode CreateListFolder(ChildInfo info)
        {
            var names = info.Node.Children().ToImmutableDictionary(x => x, SelectName);
            var nameCounts = names.Values
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x, x => 0, StringComparer.OrdinalIgnoreCase);
            var folders = info.Node.Children()
                .Select(Visit)
                .Scan(default(GitreeNode), AssignIdentifiers)
                .Skip(1)
                .ToImmutableArray();
            return new GitreeListNode(info.Name, folders);
            string GetUniqueIdentifier(GitreeNode item)
            {
                var name = names[item.WrappedNode];
                var nameIndex = ++nameCounts[name];
                var identifier = nameIndex == 1 ? name : $"{name} - {nameIndex}";
                return identifier;
            }
            GitreeNode AssignIdentifiers(GitreeNode prevItem, GitreeNode item)
            {
                var node = item.Node;
                var identifier = GetUniqueIdentifier(item);
                var newMeta = node.Meta
                    .WithIdentifier(identifier)
                    .WithPrevIdentifier(prevItem?.Node.Meta.Identifier);
                return item.WithNode(node.WithMeta(newMeta));
            }
        }

        private TNode DropFolders<TNode>(TNode node) where TNode : SourceNode
        {
            return (TNode)node.Accept(FolderDropper);
        }

        public static string SelectName(SourceNode node)
        {
            return node is INameableNode named ? named.Name : node.Kind.ToString();
        }

        public static DatablobNode BlobWith(SourceNode node)
        {
            return AddingToEmptyBlobRewriter.AddToEmpty(node);
        }

        public class AddingToEmptyBlobRewriter : SourceRewriter
        {
            private AddingToEmptyBlobRewriter(SourceNode nodeToAdd)
            {
                NodeToAdd = nodeToAdd;
            }

            private SourceNode NodeToAdd { get; }

            public static DatablobNode AddToEmpty(SourceNode node)
            {
                var rewriter = new AddingToEmptyBlobRewriter(node);
                return (DatablobNode)EmptyBlob.Accept(rewriter);
            }

            public override SourceNode Visit(SourceNode node)
            {
                if (!(node is IListNode list && list.ElementKind == NodeToAdd.Kind))
                {
                    return node;
                }
                // this is the list we want
                return node.Accept(this);
            }

            public override NodeList<TNode> VisitNodeList<TNode>(NodeList<TNode> list)
            {
                return list.Add((TNode)NodeToAdd);
            }
        }

        public class FolderKindDroppingRewriter : SourceRewriter
        {
            private NodeListCleaner ListCleaner { get; } = new NodeListCleaner();

            public override SourceNode Visit(SourceNode node)
            {
                if (FolderKinds.Contains(node.Kind))
                {
                    return ListCleaner.Visit(node);
                }
                return node;
            }
        }

        /// <summary>
        /// Rewrites any list as an empty list.
        /// </summary>
        public class NodeListCleaner : SourceRewriter
        {
            public override NodeList<TNode> VisitNodeList<TNode>(NodeList<TNode> list)
            {
                return default;
            }
        }
    }
}
