using System;
using System.Collections.Immutable;
using System.Threading;

namespace WarHub.ArmouryModel.Source
{
    internal sealed class LazyNodeList<TChildNode, TChildCore> : IContainer<TChildNode>
        where TChildNode : SourceNode
        where TChildCore : ICore<TChildNode>
    {
        internal LazyNodeList(ImmutableArray<TChildCore> cores, SourceNode parent)
        {
            Parent = parent;
            Cores = cores;
            Nodes = new ArrayElement<TChildNode>[Cores.Length];
            List = new NodeList<TChildNode>(this);
        }

        private SourceNode Parent { get; }

        private ArrayElement<TChildNode>[] Nodes { get; }

        internal ImmutableArray<TChildCore> Cores { get; }

        public NodeList<TChildNode> List { get; }

        public int SlotCount => Cores.Length;

        public TChildNode GetNodeSlot(int index)
        {
            ref var value = ref Nodes[index].Value;
            if (value == null)
            {
                var coreValue = Cores[index];
                Interlocked.CompareExchange(ref value, coreValue.ToNode(Parent), null);
            }
            return value;
        }

        internal class OneElementList : IContainer<TChildNode>
        {
            public OneElementList(TChildCore core, SourceNode parent)
            {
                Core = core;
                Parent = parent;
            }

            private TChildNode _node;

            public int SlotCount => 1;

            private SourceNode Parent { get; }

            private TChildCore Core { get; }

            public TChildNode GetNodeSlot(int index)
            {
                if (_node == null)
                {
                    Interlocked.CompareExchange(ref _node, Core.ToNode(Parent), null);
                }
                return _node;
            }
        }

        internal class TwoElementList : IContainer<TChildNode>
        {
            public TwoElementList(ImmutableArray<TChildCore> cores, SourceNode parent)
            {
                Cores = cores;
                Parent = parent;
            }

            private TChildNode _node0;
            private TChildNode _node1;

            public int SlotCount => 2;

            private SourceNode Parent { get; }

            private ImmutableArray<TChildCore> Cores { get; }

            public TChildNode GetNodeSlot(int index)
            {
                switch (index)
                {
                    case 0: return GetNode(ref _node0, Cores[0]);
                    case 1: return GetNode(ref _node1, Cores[1]);
                    default:
                        throw new IndexOutOfRangeException("Index was outside the bounds of the array");
                }
            }

            private TChildNode GetNode(ref TChildNode node, TChildCore core)
            {
                if (node == null)
                {
                    Interlocked.CompareExchange(ref node, core.ToNode(Parent), null);
                }
                return node;
            }
        }

        internal class ThreeElementList : IContainer<TChildNode>
        {
            public ThreeElementList(ImmutableArray<TChildCore> cores, SourceNode parent)
            {
                Cores = cores;
                Parent = parent;
            }

            private TChildNode _node0;
            private TChildNode _node1;
            private TChildNode _node2;

            public int SlotCount => 3;

            private SourceNode Parent { get; }

            private ImmutableArray<TChildCore> Cores { get; }

            public TChildNode GetNodeSlot(int index)
            {
                switch (index)
                {
                    case 0: return GetNode(ref _node0, Cores[0]);
                    case 1: return GetNode(ref _node1, Cores[1]);
                    case 2: return GetNode(ref _node2, Cores[2]);
                    default:
                        throw new IndexOutOfRangeException("Index was outside the bounds of the array");
                }
            }

            private TChildNode GetNode(ref TChildNode node, TChildCore core)
            {
                if (node == null)
                {
                    Interlocked.CompareExchange(ref node, core.ToNode(Parent), null);
                }
                return node;
            }
        }
    }

    internal static class LazyNodeListExtensions
    {
        public static NodeList<TNode> ToNodeList<TNode, TCore>(this ImmutableArray<TCore> cores, SourceNode parent = null)
            where TCore : ICore<TNode>
            where TNode : SourceNode, INodeWithCore<TCore>
        {
            switch (cores.Length)
            {
                case 0: return default;
                case 1: return new NodeList<TNode>(new LazyNodeList<TNode, TCore>.OneElementList(cores[0], parent));
                case 2: return new NodeList<TNode>(new LazyNodeList<TNode, TCore>.TwoElementList(cores, parent));
                case 3: return new NodeList<TNode>(new LazyNodeList<TNode, TCore>.ThreeElementList(cores, parent));
                default: return new NodeList<TNode>(new LazyNodeList<TNode, TCore>(cores, parent));
            }
        }
    }
}
