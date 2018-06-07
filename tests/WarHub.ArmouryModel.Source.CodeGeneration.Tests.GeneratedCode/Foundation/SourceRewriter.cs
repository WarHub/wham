using System.Collections.Immutable;
using System.Linq;

namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    public abstract partial class SourceRewriter : SourceVisitor<SourceNode>
    {
        public virtual NodeList<TNode> VisitNodeList<TNode>(NodeList<TNode> list) where TNode : SourceNode
        {
            ImmutableArray<TNode>.Builder alternate = null;
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var item = list[i];
                var visited = VisitListElement(item);
                if (item != visited && alternate == null)
                {
                    alternate = ImmutableArray.CreateBuilder<TNode>(n);
                    alternate.AddRange(list.Take(i));
                }

                if (alternate != null && visited != null && !visited.IsKind(SourceKind.Unknown))
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
