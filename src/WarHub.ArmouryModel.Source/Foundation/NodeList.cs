using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Diagnostics;

namespace WarHub.ArmouryModel.Source
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public partial struct NodeList<TNode> : IReadOnlyList<TNode>, IContainerProvider<TNode>
        where TNode : SourceNode
    {
        // TODO a lot of optimizations here

        internal NodeList(IContainer<TNode> container)
        {
            Container = container;
        }

        internal IContainer<TNode> Container { get; }

        public TNode this[int index] => Container.GetNodeSlot(index);

        public int Count => Container?.SlotCount ?? 0;

        IContainer<TNode> IContainerProvider<TNode>.Container => Container;

        public IEnumerator<TNode> GetEnumerator()
        {
            var count = Count;
            for (var i = 0; i < count; i++)
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

    // equality implementation
    partial struct NodeList<TNode> : IEquatable<NodeList<TNode>>
    {
        public bool Equals(NodeList<TNode> other)
        {
            return Equals(Container, other.Container);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            return obj is NodeList<TNode> list && Equals(list);
        }

        public override int GetHashCode()
        {
            return Container?.GetHashCode() ?? 0;
        }

        public static bool operator ==(NodeList<TNode> left, NodeList<TNode> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NodeList<TNode> left, NodeList<TNode> right)
        {
            return !(left == right);
        }
    }

    partial struct NodeList<TNode>
    {
        public NodeList<TNode> Add(TNode item)
        {
            return this.Append(item).ToNodeList();
        }
        public NodeList<TNode> AddRange(IEnumerable<TNode> items)
        {
            return this.Concat(items).ToNodeList();
        }
    }
    internal interface IContainerProvider<out TNode> where TNode : SourceNode
    {
        IContainer<TNode> Container { get; }
    }


    public static class NodeList
    {
        /// <summary>
        /// Creates a node list from an array of nodes.
        /// </summary>
        /// <typeparam name="TNode">
        /// Type of nodes passed, and by extension, type argument of the returned node list.
        /// </typeparam>
        /// <param name="nodes">Array of nodes be contained in node list.</param>
        /// <returns>Created node list containing all of <paramref name="nodes"/>.</returns>
        public static NodeList<TNode> Create<TNode>(params TNode[] nodes)
            where TNode : SourceNode
        {
            if (nodes.Length == 0)
            {
                return default;
            }
            var nodeArray = nodes.ToImmutableArray();
            return new NodeList<TNode>(CreateContainerForNodeArray(nodeArray));
        }

        /// <summary>
        /// Creates a node list from a sequence of nodes.
        /// </summary>
        /// <typeparam name="TNode">
        /// Type of nodes passed, and by extension, type argument of the returned node list.
        /// </typeparam>
        /// <param name="nodes">Sequence of nodes be contained in node list.</param>
        /// <returns>Created node list containing all of <paramref name="nodes"/>.</returns>
        public static NodeList<TNode> Create<TNode>(IEnumerable<TNode> nodes)
            where TNode : SourceNode
        {
            if (nodes is IContainerProvider<TNode> nodeList)
            {
                return new NodeList<TNode>(nodeList.Container);
            }
            var nodeArray = nodes.ToImmutableArray();
            if (nodeArray.Length == 0)
            {
                return default;
            }
            return new NodeList<TNode>(CreateContainerForNodeArray(nodeArray));
        }

        /// <summary>
        /// Simple wrapper of <see cref="Create{TNode}(TNode[])"/>.
        /// Creates a node list from an array of nodes.
        /// </summary>
        /// <typeparam name="TNode">
        /// Type of nodes passed, and by extension, type argument of the returned node list.
        /// </typeparam>
        /// <param name="nodes">Array of nodes be contained in node list.</param>
        /// <returns>Created node list containing all of <paramref name="nodes"/>.</returns>
        public static NodeList<TNode> ToNodeList<TNode>(this TNode[] nodes)
            where TNode : SourceNode
        {
            return NodeList.Create(nodes);
        }

        /// <summary>
        /// Simple wrapper of <see cref="Create{TNode}(IEnumerable{TNode})"/>.
        /// Creates a node list from a sequence of nodes.
        /// </summary>
        /// <typeparam name="TNode">
        /// Type of nodes passed, and by extension, type argument of the returned node list.
        /// </typeparam>
        /// <param name="nodes">Sequence of nodes be contained in node list.</param>
        /// <returns>Created node list containing all of <paramref name="nodes"/>.</returns>
        public static NodeList<TNode> ToNodeList<TNode>(this IEnumerable<TNode> nodes)
            where TNode : SourceNode
        {
            return NodeList.Create(nodes);
        }

        internal static ImmutableArray<TCore> ToCoreArray<TCore, TNode>(this NodeList<TNode> list)
            where TNode : SourceNode, INodeWithCore<TCore>
            where TCore : ICore<TNode>
        {
            // shortcuts for easy cases
            if (list.Count == 0)
            {
                return ImmutableArray<TCore>.Empty;
            }
            if (list.Container is INodeListWithCoreArray<TNode, TCore> nodeListCore)
            {
                return nodeListCore.Cores;
            }
            // manual recovery of cores
            var count = list.Count;
            var builder = ImmutableArray.CreateBuilder<TCore>(count);
            for (int i = 0; i < count; i++)
            {
                INodeWithCore<TCore> item = list[i];
                builder.Add(item.Core);
            }
            return builder.MoveToImmutable();
        }

        private static IContainer<TNode> CreateContainerForNodeArray<TNode>(ImmutableArray<TNode> nodes)
            where TNode : SourceNode
        {
            return new NodeCollectionContainerProxy<TNode>(nodes);
        }
    }
}
