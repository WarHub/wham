// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface IEntriesLinkedNodeContainer
    {
        INodeSimple<IEntry> Entries { get; }

        INode<IEntryLink, IEntry> EntryLinks { get; }
    }
}
