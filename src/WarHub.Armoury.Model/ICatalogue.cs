// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface ICatalogue : ICatalogueBase, IRulesLinkedNodeContainer,
        ICatalogueContextProvider
    {
        INodeSimple<IRootEntry> Entries { get; }

        INode<IRootLink, IEntry> EntryLinks { get; }

        IIdLink<IGameSystem> GameSystemLink { get; }

        INodeSimple<IEntry> SharedEntries { get; }

        INodeSimple<IGroup> SharedGroups { get; }

        INodeSimple<IProfile> SharedProfiles { get; }

        INodeSimple<IRule> SharedRules { get; }

        IGameSystemContext SystemContext { get; set; }
    }
}
