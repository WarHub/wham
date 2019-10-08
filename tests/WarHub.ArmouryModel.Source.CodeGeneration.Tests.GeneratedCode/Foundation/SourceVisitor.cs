namespace WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode
{
    public abstract partial class SourceVisitor
    {
        public virtual void Visit(SourceNode node)
        {
            node?.Accept(this);
        }

        public virtual void DefaultVisit(SourceNode node)
        {
        }
    }

    public abstract partial class SourceVisitor<TResult>
    {
        public virtual TResult Visit(SourceNode node)
        {
            if (node != null)
            {
                return node.Accept(this);
            }
            return default;
        }

        public virtual TResult DefaultVisit(SourceNode node)
        {
            return default;
        }
    }
}
