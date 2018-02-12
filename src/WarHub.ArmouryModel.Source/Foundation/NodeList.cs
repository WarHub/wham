using System.Collections.Generic;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    public partial struct NodeList<TItem> : IReadOnlyList<TItem> where TItem : SourceNode
    {
        // TODO a lot of optimizations here

        internal NodeList(IContainer<TItem> container)
        {
            Container = container;
        }

        internal IContainer<TItem> Container { get; }

        public TItem this[int index] => Container.GetNodeSlot(index);

        public int Count => Container?.SlotCount ?? 0;

        public IEnumerator<TItem> GetEnumerator()
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    partial struct NodeList<TItem>
    {
        public NodeList<TItem> AddRange(IEnumerable<TItem> items)
        {
            return this.Concat(items).ToNodeList();
        }
    }

    public static class NodeList
    {
        public static NodeList<TItem> Create<TItem>(IEnumerable<TItem> items)
            where TItem : SourceNode
        {
            return new NodeList<TItem>(new NodeCollectionContainerProxy<TItem>(items.ToImmutableArray()));
        }

        public static NodeList<TItem> ToNodeList<TItem>(this IEnumerable<TItem> items)
            where TItem : SourceNode
        {
            return NodeList.Create(items);
        }

        internal static ImmutableArray<TCore> ToCoreArray<TCore, TNode>(this NodeList<TNode> array)
            where TNode : SourceNode, INodeWithCore<TCore>
            where TCore : ICore<TNode>
        {
            // shortcut for easy case
            if (array.Container is LazyNodeList<TNode, TCore> nodeListCore)
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
