using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    public static class SourceExtensions
    {
        public static bool IsKind(this SourceNode @this, SourceKind kind) => @this.Kind == kind;

        /// <summary>
        /// Retrieves child info for this node from Parent node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static ChildInfo? GetChildInfoFromParent(this SourceNode node)
        {
            return node.Parent?.ChildrenInfos().ElementAt(node.IndexInParent);
        }
    }
}
