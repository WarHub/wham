using System.Collections.Immutable;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    /// Represents a <see cref="SourceVisitor{TResult}"/> which descends
    /// an entire <see cref="SourceNode"/> graph and may replace or remove
    /// visited SyntaxNodes in depth-first order. Adds three methods
    /// for easier rewriting of ListNodes:
    /// <see cref="VisitListNode{TNode}(ListNode{TNode})"/>,
    /// <see cref="VisitNodeList{TNode}(NodeList{TNode})"/>
    /// and <see cref="VisitListElement{TNode}(TNode)"/>.
    public abstract partial class SourceRewriter : SourceVisitor<SourceNode?>
    {
        /// <summary>
        /// Returns a rewritten instance of given <see cref="ListNode{T}"/>.
        /// </summary>
        /// <typeparam name="TNode">Type of nodes in the list.</typeparam>
        /// <param name="list">The list to be rewritten.</param>
        /// <returns>The rewritten list.</returns>
        public virtual ListNode<TNode> VisitListNode<TNode>(ListNode<TNode> list)
            where TNode : SourceNode
        {
            return list.WithNodes(VisitNodeList(list.NodeList));
        }

        /// <summary>
        /// Rewrite all elements in the list and build up a new one
        /// from returned elements, skipping those that returned <c>null</c>.
        /// </summary>
        /// <typeparam name="TNode">Type of nodes in the list.</typeparam>
        /// <param name="list">List of nodes to rewrite.</param>
        /// <returns>A new, rewritten list of nodes.</returns>
        public virtual NodeList<TNode> VisitNodeList<TNode>(NodeList<TNode> list)
            where TNode : SourceNode
        {
            ImmutableArray<TNode>.Builder? builder = null;
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var original = list[i];
                var returned = VisitListElement(original);
                var itemChanged = original != returned;
                if (itemChanged && builder is null)
                {
                    builder = ImmutableArray.CreateBuilder<TNode>(n);
                    builder.AddRange(list.Take(i));
                }
                if (itemChanged || builder is { })
                {
                    // in this 'if' builder is not null:
                    // if itemChanged, builder was created in the above 'if' for sure
                    // else, builder is { }
                    if (returned is { })
                    {
                        builder!.Add(returned);
                    }
                }
            }
            return
                builder is null
                    ? list
                    : builder.Capacity == builder.Count
                        ? builder.MoveToImmutable().ToNodeList()
                        : builder.ToImmutable().ToNodeList();
        }

        /// <summary>
        /// Rewrites a single element of <see cref="NodeList{TNode}"/>.
        /// Returning <c>null</c> will remove the element.
        /// </summary>
        /// <typeparam name="TNode">Type of node to rewrite.</typeparam>
        /// <param name="node">Node to rewrite.</param>
        /// <returns>A new node to replace the old node with,
        /// or <c>null</c> to remove it.</returns>
        public virtual TNode? VisitListElement<TNode>(TNode? node) where TNode : SourceNode
        {
            return (TNode?)Visit(node);
        }
    }
}
