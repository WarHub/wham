// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTree
{
    using System.Collections.Generic;
    using System.Linq;

    public static class NodeExtensions
    {
        public static IEnumerable<INode> DescendantLinkNodes(this INode @this)
        {
            return
                @this.Children.SelectMany(
                    node => node.IsLinkNode ? new[] {node} : node.DescendantLinkNodes());
        }

        public static IEnumerable<IGroupNode> DescendantNotLinkGroupNodes(this INode @this)
        {
            return
                @this.GroupNodes.SelectMany(
                    node =>
                        node.IsLinkNode ? new IGroupNode[0] : node.DescendantNotLinkGroupNodes().PrependWith(node));
        }

        public static IEnumerable<INode> Parents(this INode @this)
        {
            while (true)
            {
                if (@this.IsRoot)
                    yield break;
                yield return @this.Parent;
                @this = @this.Parent;
            }
        }
    }
}
