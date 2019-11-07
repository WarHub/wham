using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    /// <summary>
    /// Reads <see cref="GitreeNode"/> and converts to appropriate <see cref="SourceNode"/>.
    /// </summary>
    internal class GitreeToSourceNodeConverter
    {
        static GitreeToSourceNodeConverter()
        {
            InvertedChildListAliases = Gitree.ChildListAliases
                .ToImmutableDictionary(x => x.Value, x => x.Key);
        }

        /// <summary>
        /// Gets mapping of child aliases to full child names.
        /// </summary>
        private static ImmutableDictionary<string, string> InvertedChildListAliases { get; }

        public SourceNode ParseNode(GitreeNode gitreeNode)
        {
            var lists = ParseLists(gitreeNode.Lists);
            var node = gitreeNode.WrappedNode;
            var assigner = new ListsAssigner(lists);
            return assigner.Visit(node);
        }

        private ImmutableDictionary<string, ImmutableArray<SourceNode>> ParseLists(ImmutableArray<GitreeListNode> lists)
        {
            return lists
                .SelectMany(x =>
                {
                    var nodes = ParseList(x);
                    return GetAliases(x.Name)
                    .Select(alias => (alias, nodes).ToKeyValuePair());
                })
                .ToImmutableDictionary();

            static IEnumerable<string> GetAliases(string listName)
            {
                yield return listName;
                if (InvertedChildListAliases.TryGetValue(listName, out var alias))
                {
                    yield return alias;
                }
            }
        }

        private ImmutableArray<SourceNode> ParseList(GitreeListNode blobList)
        {
            var kind = InvertedChildListAliases[blobList.Name];
            var n = blobList.Items.Length;
            if (n == 0)
            {
                return ImmutableArray<SourceNode>.Empty;
            }
            // wrapping int? into ValueTuple to enable null-key.
            var nodesByPrevIdentifier = blobList.Items
                .ToImmutableDictionary(node => ValueTuple.Create(node.Datablob.Meta.PrevIdentifier));
            var orderedList = ImmutableArray.CreateBuilder<SourceNode>(blobList.Items.Length);
            string prevId = null;
            for (var i = 0; i < n; i++)
            {
                var gitreeNode = nodesByPrevIdentifier[ValueTuple.Create(prevId)];
                prevId = gitreeNode.Datablob.Meta.Identifier;
                var node = ParseNode(gitreeNode);
                orderedList.Add(node);
            }
            return orderedList.MoveToImmutable();
        }

        private class ListsAssigner : SourceRewriter
        {
            public ListsAssigner(ImmutableDictionary<string, ImmutableArray<SourceNode>> lists)
            {
                Lists = lists;
            }

            public override ListNode<TNode> VisitListNode<TNode>(ListNode<TNode> listNode)
            {
                if (!Gitree.SeparatableKinds.Contains(listNode.Kind))
                {
                    return listNode;
                }
                // this is a separatable list
                var childName = listNode.GetChildInfoFromParent().Name;
                var nodes = Lists.TryGetValue(childName, out var value)
                    ? value : ImmutableArray<SourceNode>.Empty;
                return listNode.WithNodes(nodes.Cast<TNode>().ToNodeList());
            }

            public ImmutableDictionary<string, ImmutableArray<SourceNode>> Lists { get; }
        }
    }
}
