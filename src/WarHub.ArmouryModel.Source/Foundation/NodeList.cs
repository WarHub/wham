using System.Collections.Generic;
using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// Contains factory and extension methods for creating <see cref="NodeList{TNode}"/>
    /// </summary>
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
            return CreateContainerForNodeArray(nodeArray).ToNodeList();
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
            if (nodes is IContainerProvider<TNode> containerProvider)
            {
                return containerProvider.Container.ToNodeList();
            }
            var nodeArray = nodes.ToImmutableArray();
            if (nodeArray.Length == 0)
            {
                return default;
            }
            return CreateContainerForNodeArray(nodeArray).ToNodeList();
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
            where TCore : ICore<TNode>
            where TNode : SourceNode, INodeWithCore<TCore>
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
            for (var i = 0; i < count; i++)
            {
                INodeWithCore<TCore> item = list[i];
                builder.Add(item.Core);
            }
            return builder.MoveToImmutable();
        }

        internal static NodeList<TNode> ToNodeList<TNode>(this IContainer<TNode>? container)
            where TNode : SourceNode
            => new NodeList<TNode>(container);

        private static IContainer<TNode> CreateContainerForNodeArray<TNode>(ImmutableArray<TNode> nodes)
            where TNode : SourceNode
        {
            return new NodeCollectionContainerProxy<TNode>(nodes);
        }
    }
}
