using System.Collections.Immutable;

namespace WarHub.ArmouryModel.Source
{
    internal static class LazyNodeListExtensions
    {
        public static NodeList<TNode> ToNodeList<TNode, TCore>(this ImmutableArray<TCore> cores, SourceNode? parent = null)
            where TNode : SourceNode, INodeWithCore<TCore>
            where TCore : ICore<TNode>
        {
            var container = LazyNodeList<TNode, TCore>.CreateContainer(cores, parent);
            return container.ToNodeList();
        }
    }
}
