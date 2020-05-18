using System.Collections.Immutable;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// Represents a <see cref="SourceVisitor{TResult}"/> which descends and entire <see cref="SourceNode"/> graph and
    /// may replace or remove visited SyntaxNodes in depth-first order.
    /// </summary>
    public abstract partial class SourceRewriter : SourceVisitor<SourceNode>
    {
        public virtual ListNode<TNode> VisitListNode<TNode>(ListNode<TNode> list)
            where TNode : SourceNode
        {
            return list.WithNodes(VisitNodeList(list.NodeList));
        }

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

        public virtual TNode? VisitListElement<TNode>(TNode? node) where TNode : SourceNode
        {
            return (TNode?)Visit(node);
        }
    }
}
