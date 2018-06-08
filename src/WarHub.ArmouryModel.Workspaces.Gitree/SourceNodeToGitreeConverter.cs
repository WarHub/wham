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
            NonLeafKinds =
                new[]
                {
                    SourceKind.Catalogue,
                    SourceKind.Datablob,
                    SourceKind.ForceEntry,
                    SourceKind.Force,
                    SourceKind.Gamesystem,
                    SourceKind.Roster,
                    SourceKind.SelectionEntryGroup,
                    SourceKind.SelectionEntry,
                    SourceKind.Selection
                }.ToImmutableHashSet();

            SeparatableKinds =
                NonLeafKinds
                .Concat(new[]
                {
                    SourceKind.DataIndex,
                    SourceKind.Profile,
                    SourceKind.ProfileType,
                    SourceKind.Rule
                })
                .ToImmutableHashSet();
        }

        public static ImmutableHashSet<SourceKind> NonLeafKinds { get; }

        /// <summary>
        /// Gets a set of source kinds that will be separated from the entity into child folders.
        /// </summary>
        public static ImmutableHashSet<SourceKind> SeparatableKinds { get; }

        private SeparatableDropperRewriter SeparatableDropper { get; } = new SeparatableDropperRewriter();

        private static DatablobNode EmptyBlob { get; }
            = NodeFactory.Datablob(NodeFactory.Metadata(null, null, null));

        public override GitreeNode DefaultVisit(SourceNode node)
        {
            var blob = BlobWith(node);
            return GitreeNode.CreateLeaf(blob, node);
        }

        public override GitreeNode Visit(SourceNode node)
        {
            if (NonLeafKinds.Contains(node.Kind))
            {
                return VisitNonLeafNode(node);
            }
            return base.Visit(node);
        }

        private GitreeNode VisitNonLeafNode(SourceNode node)
        {
            var listFolders = CreateLists(node);
            var strippedNode = DropSeparatableChildren(node);
            var blob = BlobWith(strippedNode);
            return GitreeNode.CreateNonLeaf(blob, node, listFolders);
        }

        private ImmutableArray<GitreeListNode> CreateLists(SourceNode node)
        {
            var listFolders = node
                .ChildrenInfos()
                .Where(IsSeparatable)
                .Select(CreateList)
                .ToImmutableArray();
            return listFolders;
            bool IsSeparatable(ChildInfo info)
            {
                return info.IsList
                    && info.Node is IListNode list
                    && SeparatableKinds.Contains(list.ElementKind);
            }
        }

        private GitreeListNode CreateList(ChildInfo info)
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

        private SourceNode DropSeparatableChildren(SourceNode node)
        {
            return node.Accept(SeparatableDropper);
        }

        public static string SelectName(SourceNode node)
        {
            return node is INameableNode named ? named.Name : node.Kind.ToString();
        }

        public static DatablobNode BlobWith(SourceNode node)
        {
            return AddingToEmptyBlobRewriter.AddToEmpty(node);
        }

        /// <summary>
        /// Visitor that adds visited node to appropriate list
        /// of an empty <see cref="DatablobNode"/> which is returned.
        /// </summary>
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

        /// <summary>
        /// When a node accepts this rewriter, all of it's children <see cref="IListNode"/>s
        /// that contains elements of <see cref="SeparatableKinds"/> are rewritten as empty.
        /// Don't call <see cref="Visit(SourceNode)"/> directly, because it only doesn't visit
        /// node's children - either returns the same node, or if it's a node described above,
        /// returns empty list node.
        /// </summary>
        public class SeparatableDropperRewriter : SourceRewriter
        {
            private NodeListCleaner ListCleaner { get; } = new NodeListCleaner();

            public override SourceNode Visit(SourceNode node)
            {
                if (node.IsList && node is IListNode list && SeparatableKinds.Contains(list.ElementKind))
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
