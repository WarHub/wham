namespace WarHub.ArmouryModel.Source
{
    public interface ICore<out TNode> where TNode : SourceNode
    {
        TNode ToNode(SourceNode parent = null);

        // TODO uncomment after generation implemented

        //Type GetSerializationProxyType();

        //Type GetBuilderType();

        //Type GetSerializationEnumerableType();
    }

    internal struct Witness<TCore, TBuilder, TProxy, TNode>
        where TCore : NodeCore, ICore<TNode>, IBuildable<TCore, TBuilder>
        where TBuilder : IBuilder<TCore>
        where TNode : SourceNode, INodeWithCore<TCore>
    {
        public TCore ToCore(TBuilder builder) => builder.ToImmutable();

        public TCore ToCore(TNode node) => ((INodeWithCore<TCore>)node).Core;

        public TNode ToNode(TCore core, SourceNode parent = null)
            => ((ICore<TNode>)core).ToNode(parent);

        public TBuilder ToBuilder(TCore core) => core.ToBuilder();

        //public TProxy ToProxy(TCore core) => core.ToSerializationProxy();
    }
}
