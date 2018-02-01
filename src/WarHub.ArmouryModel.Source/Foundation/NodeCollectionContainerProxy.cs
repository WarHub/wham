using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    internal struct NodeCollectionContainerProxy<TChildNode> : IContainer<TChildNode>
        where TChildNode : SourceNode
    {
        public NodeCollectionContainerProxy(ImmutableArray<TChildNode> nodes)
        {
            Nodes = nodes;
        }

        internal ImmutableArray<TChildNode> Nodes { get; }

        public int SlotCount => Nodes.Length;

        public TChildNode GetNodeSlot(int index)
        {
            return Nodes[index];
        }
    }
}
