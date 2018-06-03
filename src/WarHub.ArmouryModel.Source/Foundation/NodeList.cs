using System.Collections.Generic;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Diagnostics;

namespace WarHub.ArmouryModel.Source
{
    [DebuggerDisplay("Count = {Count}")]
    public partial struct NodeList<TNode> : IReadOnlyList<TNode> where TNode : SourceNode
    {
        // TODO a lot of optimizations here

        internal NodeList(IContainer<TNode> container)
        {
            Container = container;
        }

        internal IContainer<TNode> Container { get; }

        public TNode this[int index] => Container.GetNodeSlot(index);

        public int Count => Container?.SlotCount ?? 0;

        public IEnumerator<TNode> GetEnumerator()
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        public static implicit operator NodeList<SourceNode>(NodeList<TNode> nodeList)
        {
            return new NodeList<SourceNode>(nodeList.Container);
        }

        public static implicit operator NodeList<TNode>(NodeList<SourceNode> nodeList)
        {
            return new NodeList<TNode>((IContainer<TNode>)nodeList.Container);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    partial struct NodeList<TNode>
    {
        public NodeList<TNode> AddRange(IEnumerable<TNode> items)
        {
            return this.Concat(items).ToNodeList();
        }
    }

    public static class NodeList
    {
        public static NodeList<TNode> Create<TNode>(IEnumerable<TNode> items)
            where TNode : SourceNode
        {
            return new NodeList<TNode>(new NodeCollectionContainerProxy<TNode>(items.ToImmutableArray()));
        }

        public static NodeList<TNode> ToNodeList<TNode>(this IEnumerable<TNode> items)
            where TNode : SourceNode
        {
            return NodeList.Create(items);
        }

        internal static ImmutableArray<TCore> ToCoreArray<TCore, TNode>(this NodeList<TNode> array)
            where TNode : SourceNode, INodeWithCore<TCore>
            where TCore : ICore<TNode>
        {
            // shortcut for easy case
            if (array.Container is INodeListWithCoreArray<TNode, TCore> nodeListCore)
            {
                return nodeListCore.Cores;
            }
            var count = array.Count;
            var builder = ImmutableArray.CreateBuilder<TCore>(count);
            for (int i = 0; i < count; i++)
            {
                INodeWithCore<TCore> item = array[i];
                builder.Add(item.Core);
            }
            return builder.MoveToImmutable();
        }
    }
}
