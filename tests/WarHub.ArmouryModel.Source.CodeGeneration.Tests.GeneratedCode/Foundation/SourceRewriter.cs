using System.Collections.Immutable;
using System.Linq;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
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

        public virtual NodeList<TNode> VisitNodeList<TNode>(NodeList<TNode> list) where TNode : SourceNode
        {
            ImmutableArray<TNode>.Builder alternate = null;
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var item = list[i];
                var visited = VisitListElement(item);
                if (alternate is null)
                {
                    if (item != visited)
                    {
                        alternate = ImmutableArray.CreateBuilder<TNode>(n);
                        alternate.AddRange(list.Take(i));
                    }
                }
                else if (visited?.IsKind(SourceKind.Unknown) == false)
                {
                    alternate.Add(visited);
                }
            }
            return alternate?.MoveToImmutable().ToNodeList() ?? list;
        }

        public virtual TNode VisitListElement<TNode>(TNode node) where TNode : SourceNode
        {
            return (TNode)Visit(node);
        }
    }
}
