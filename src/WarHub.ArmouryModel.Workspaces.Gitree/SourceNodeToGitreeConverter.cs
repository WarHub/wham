using System;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq;
using Optional;
using Optional.Collections;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    internal class SourceNodeToGitreeConverter : SourceVisitor<GitreeNode>
    {
        private SeparatableChildrenRemover SeparatableRemover { get; }
            = new SeparatableChildrenRemover();

        private static DatablobNode EmptyBlob { get; }
            = NodeFactory.Datablob(NodeFactory.Metadata(null, null, null));

        public override GitreeNode Visit(SourceNode node)
        {
            var listFolders = CreateLists(node);
            var strippedNode = listFolders.IsEmpty ? node : DropSeparatableChildren(node);
            var blob = BlobWith(strippedNode);
            return GitreeNode.Create(blob, node, listFolders);
        }

        private ImmutableArray<GitreeListNode> CreateLists(SourceNode node)
        {
            return node
                .ChildrenInfos()
                .Select(CreateListOption)
                .Values()
                .ToImmutableArray();

            Option<GitreeListNode> CreateListOption(ChildInfo info)
            {
                if (info.IsList
                    && Gitree.SeparatableKinds.Contains(info.Node.Kind)
                    && info.Node is IListNode list)
                {
                    var treeNodes = CreateList(list.NodeList);
                    var name = Gitree.ChildListAliases.TryGetValue(info.Name, out var alias)
                        ? alias : info.Name;
                    return new GitreeListNode(name) { Items = treeNodes }.Some();
                }
                return default;
            }
        }

        private ImmutableArray<GitreeNode> CreateList(NodeList<SourceNode> nodes)
        {
            var names = nodes.ToImmutableDictionary(x => x, SelectName);
            var nameCounts = names.Values
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x, _ => 0, StringComparer.OrdinalIgnoreCase);
            return nodes
                .Select(Visit)
                .Scan(default(GitreeNode), AssignIdentifiers)
                // skip seed
                .Skip(1)
                .ToImmutableArray();

            GitreeNode AssignIdentifiers(GitreeNode prevTreeNode, GitreeNode treeNode)
            {
                var blob = treeNode.Datablob;
                var identifier = GetUniqueIdentifier(treeNode.WrappedNode);
                var newMeta = blob.Meta
                    .WithIdentifier(identifier)
                    .WithPrevIdentifier(prevTreeNode?.Datablob.Meta.Identifier);
                return treeNode with { Datablob = blob.WithMeta(newMeta) };
            }

            string GetUniqueIdentifier(SourceNode node)
            {
                var name = names[node];
                var repetitions = nameCounts[name]++;
                return repetitions == 0 ? name : $"{name} - {repetitions}";
            }
        }

        private SourceNode DropSeparatableChildren(SourceNode node)
        {
            return node.Accept(SeparatableRemover);
        }

        private static string SelectName(SourceNode node)
        {
            var name = node is INameableNode named ? named.Name : node.Kind.ToString();
            return name.FilenameSanitize();
        }

        private static DatablobNode BlobWith(SourceNode node)
        {
            return DatablobRewriter.AddToEmpty(node);
        }

        /// <summary>
        /// Visitor that adds visited node to appropriate list
        /// of an empty <see cref="DatablobNode"/> which is returned.
        /// </summary>
        public sealed class DatablobRewriter : SourceRewriter
        {
            private DatablobRewriter(SourceNode nodeToAdd)
            {
                NodeToAdd = nodeToAdd;
            }

            private SourceNode NodeToAdd { get; }

            public static DatablobNode AddToEmpty(SourceNode node)
            {
                return (DatablobNode)EmptyBlob.Accept(new DatablobRewriter(node));
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

            public override ListNode<TNode> VisitListNode<TNode>(ListNode<TNode> list)
            {
                return list.ElementKind == NodeToAdd.Kind
                    ? list.WithNodes(NodeList.Create((TNode)NodeToAdd))
                    : list;
            }
        }

        /// <summary>
        /// All of visited node's children <see cref="ListNode{TChild}"/>s
        /// of <see cref="Gitree.SeparatableKinds"/> are rewritten as empty.
        /// These lists' children are not visited.
        /// </summary>
        public class SeparatableChildrenRemover : SourceRewriter
        {
            public override ListNode<TNode> VisitListNode<TNode>(ListNode<TNode> list)
            {
                if (Gitree.SeparatableKinds.Contains(list.Kind))
                {
                    return list.WithNodes(default);
                }
                return base.VisitListNode(list);
            }
        }
    }
}
