using System;
using System.Collections.Immutable;
using System.Threading;

namespace WarHub.ArmouryModel.Source
{
    internal sealed class LazyNodeList<TNode, TCore>
        : IContainer<TNode>, INodeListWithCoreArray<TNode, TCore>
        where TNode : SourceNode, INodeWithCore<TCore>
        where TCore : ICore<TNode>
    {
        private LazyNodeList(ImmutableArray<TCore> cores, SourceNode parent)
        {
            Parent = parent;
            Cores = cores;
            Nodes = new ArrayElement<TNode>[Cores.Length];
        }

        private SourceNode Parent { get; }

        private ArrayElement<TNode>[] Nodes { get; }

        public ImmutableArray<TCore> Cores { get; }

        public int SlotCount => Cores.Length;

        public TNode GetNodeSlot(int index)
        {
            ref var value = ref Nodes[index].Value;
            if (value == null)
            {
                var coreValue = Cores[index];
                Interlocked.CompareExchange(ref value, coreValue.ToNode(Parent), null);
                value.IndexInParentLazy = index;
            }
            return value;
        }

        private class OneElementList : IContainer<TNode>, INodeListWithCoreArray<TNode, TCore>
        {
            public OneElementList(ImmutableArray<TCore> cores, SourceNode parent)
            {
                Cores = cores;
                Parent = parent;
            }

            private TNode node;

            public int SlotCount => 1;

            public SourceNode Parent { get; }

            public ImmutableArray<TCore> Cores { get; }

            public TNode GetNodeSlot(int index)
            {
                if (node == null)
                {
                    Interlocked.CompareExchange(ref node, Cores[0].ToNode(Parent), null);
                    node.IndexInParentLazy = index;
                }
                return node;
            }
        }

        private class TwoElementList : IContainer<TNode>, INodeListWithCoreArray<TNode, TCore>
        {
            public TwoElementList(ImmutableArray<TCore> cores, SourceNode parent)
            {
                Cores = cores;
                Parent = parent;
            }

            private TNode node0;
            private TNode node1;

            public int SlotCount => 2;

            public SourceNode Parent { get; }

            public ImmutableArray<TCore> Cores { get; }

            public TNode GetNodeSlot(int index)
            {
                return index switch
                {
                    0 => GetNode(ref node0, 0),
                    1 => GetNode(ref node1, 1),
                    _ => throw new IndexOutOfRangeException("Index was outside the bounds of the array"),
                };
            }

            private TNode GetNode(ref TNode node, int index)
            {
                if (node == null)
                {
                    Interlocked.CompareExchange(ref node, Cores[index].ToNode(Parent), null);
                    node.IndexInParentLazy = index;
                }
                return node;
            }
        }

        private class ThreeElementList : IContainer<TNode>, INodeListWithCoreArray<TNode, TCore>
        {
            public ThreeElementList(ImmutableArray<TCore> cores, SourceNode parent)
            {
                Cores = cores;
                Parent = parent;
            }

            private TNode node0;
            private TNode node1;
            private TNode node2;

            public int SlotCount => 3;

            public SourceNode Parent { get; }

            public ImmutableArray<TCore> Cores { get; }

            public TNode GetNodeSlot(int index)
            {
                return index switch
                {
                    0 => GetNode(ref node0, 0),
                    1 => GetNode(ref node1, 1),
                    2 => GetNode(ref node2, 2),
                    _ => throw new IndexOutOfRangeException("Index was outside the bounds of the array"),
                };
            }

            private TNode GetNode(ref TNode node, int index)
            {
                if (node == null)
                {
                    Interlocked.CompareExchange(ref node, Cores[index].ToNode(Parent), null);
                    node.IndexInParentLazy = index;
                }
                return node;
            }
        }

        internal static IContainer<TNode> CreateContainer(ImmutableArray<TCore> cores, SourceNode parent)
        {
            return cores.Length switch
            {
                0 => default,
                1 => new OneElementList(cores, parent),
                2 => new TwoElementList(cores, parent),
                3 => new ThreeElementList(cores, parent),
                _ => new LazyNodeList<TNode, TCore>(cores, parent),
            };
        }
    }

    internal static class LazyNodeListExtensions
    {
        public static NodeList<TNode> ToNodeList<TNode, TCore>(this ImmutableArray<TCore> cores, SourceNode parent = null)
            where TNode : SourceNode, INodeWithCore<TCore>
            where TCore : ICore<TNode>
        {
            var container = LazyNodeList<TNode, TCore>.CreateContainer(cores, parent);
            return new NodeList<TNode>(container);
        }
    }
}
