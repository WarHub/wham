// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTree
{
    public interface IGroupNode : INode
    {
        IGroup Group { get; }

        GroupLinkPair GroupLinkPair { get; }

        IGroupLink Link { get; }
    }
}
