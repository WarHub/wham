using System.Diagnostics.CodeAnalysis;

namespace WarHub.ArmouryModel.Source
{
    public abstract partial class SourceVisitor
    {
        public virtual void Visit(SourceNode? node)
        {
            node?.Accept(this);
        }

        public virtual void DefaultVisit(SourceNode node)
        {
        }
    }

    public abstract partial class SourceVisitor<TResult>
    {
        [return: MaybeNull]
        public virtual TResult Visit(SourceNode? node)
        {
            if (node != null)
            {
                return node.Accept(this);
            }
            return default;
        }

        [return: MaybeNull]
        public virtual TResult DefaultVisit(SourceNode node)
        {
            return default;
        }
    }
}
