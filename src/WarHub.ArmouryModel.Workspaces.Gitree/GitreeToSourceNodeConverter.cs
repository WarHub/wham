using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    /// <summary>
    /// Reads <see cref="GitreeNode"/> and converts to appropriate <see cref="SourceNode"/>.
    /// </summary>
    internal class GitreeToSourceNodeConverter
    {
        public SourceNode ParseItem(GitreeNode blobItem)
        {
            switch (blobItem.WrappedNode.Kind)
            {
                case SourceKind.SelectionEntry:
                case SourceKind.SelectionEntryGroup:
                    return ParseSelectionEntryBase(blobItem);
                case SourceKind.ForceEntry:
                    return ParseForceEntry(blobItem);
                case SourceKind.Force:
                    return ParseForce(blobItem);
                case SourceKind.Selection:
                    return ParseSelection(blobItem);
                case SourceKind.Catalogue:
                case SourceKind.Gamesystem:
                    return ParseCatalogueBase(blobItem);
                case SourceKind.Roster:
                    return ParseRoster(blobItem);
                default:
                    return blobItem.WrappedNode;
            }
        }

        private NodeListDictionaryWithDefault ParseItemLists(GitreeNode blobItem)
        {
            var dictionary = blobItem.Lists.ToImmutableDictionary(x => x.Name, ParseList);
            var wrapper = new NodeListDictionaryWithDefault(dictionary);
            return wrapper;
        }

        private IEnumerable<SourceNode> ParseList(GitreeListNode blobList)
        {
            if (blobList.Items.IsEmpty)
            {
                return Enumerable.Empty<SourceNode>();
            }
            var first = blobList.Items.First(x => x.Node.Meta.PrevIdentifier == null);
            var nodesByPrevIdentifier = blobList.Items
                .Remove(first)
                .ToImmutableDictionary(node => node.Node.Meta.PrevIdentifier);
            var orderedList = new List<SourceNode>
            {
                ParseItem(first)
            };
            string prevId = first.Node.Meta.Identifier;
            for (int i = 0; i < nodesByPrevIdentifier.Count; i++)
            {
                var node = nodesByPrevIdentifier[prevId];
                prevId = node.Node.Meta.Identifier;
                orderedList.Add(ParseItem(node));
            }
            return orderedList;
        }

        private CatalogueBaseNode ParseCatalogueBase(GitreeNode blobItem)
        {
            var lists = ParseItemLists(blobItem);
            var node = (CatalogueBaseNode)blobItem.WrappedNode;
            var filledNode = node
                .AddForceEntries(lists[nameof(CatalogueBaseNode.ForceEntries)].Cast<ForceEntryNode>())
                .AddProfiles(lists[nameof(CatalogueBaseNode.Profiles)].Cast<ProfileNode>())
                .AddProfileTypes(lists[nameof(CatalogueBaseNode.ProfileTypes)].Cast<ProfileTypeNode>())
                .AddRules(lists[nameof(CatalogueBaseNode.Rules)].Cast<RuleNode>())
                .AddSelectionEntries(lists[nameof(CatalogueBaseNode.SelectionEntries)].Cast<SelectionEntryNode>())
                .AddSharedProfiles(lists[nameof(CatalogueBaseNode.SharedProfiles)].Cast<ProfileNode>())
                .AddSharedRules(lists[nameof(CatalogueBaseNode.SharedRules)].Cast<RuleNode>())
                .AddSharedSelectionEntries(lists[nameof(CatalogueBaseNode.SharedSelectionEntries)].Cast<SelectionEntryNode>())
                .AddSharedSelectionEntryGroups(lists[nameof(CatalogueBaseNode.SharedSelectionEntryGroups)].Cast<SelectionEntryGroupNode>());
            return filledNode;
        }

        private ForceEntryNode ParseForceEntry(GitreeNode blobItem)
        {
            var lists = ParseItemLists(blobItem);
            var node = (ForceEntryNode)blobItem.WrappedNode;
            var filledNode = node
                .AddForceEntries(lists[nameof(ForceEntryNode.ForceEntries)].Cast<ForceEntryNode>())
                .AddProfiles(lists[nameof(ForceEntryNode.Profiles)].Cast<ProfileNode>())
                .AddRules(lists[nameof(ForceEntryNode.Rules)].Cast<RuleNode>());
            return filledNode;
        }

        private SelectionEntryBaseNode ParseSelectionEntryBase(GitreeNode blobItem)
        {
            var lists = ParseItemLists(blobItem);
            var node = (SelectionEntryBaseNode)blobItem.WrappedNode;
            var filledNode = node
                .AddProfiles(lists[nameof(SelectionEntryBaseNode.Profiles)].Cast<ProfileNode>())
                .AddRules(lists[nameof(SelectionEntryBaseNode.Rules)].Cast<RuleNode>())
                .AddSelectionEntries(lists[nameof(SelectionEntryBaseNode.SelectionEntries)].Cast<SelectionEntryNode>())
                .AddSelectionEntryGroups(lists[nameof(SelectionEntryBaseNode.SelectionEntryGroups)].Cast<SelectionEntryGroupNode>());
            return filledNode;
        }

        private RosterNode ParseRoster(GitreeNode blobItem)
        {
            var lists = ParseItemLists(blobItem);
            var node = (RosterNode)blobItem.WrappedNode;
            var filledNode = node
                .AddForces(lists[nameof(RosterNode.Forces)].Cast<ForceNode>());
            return filledNode;
        }

        private ForceNode ParseForce(GitreeNode blobItem)
        {
            var lists = ParseItemLists(blobItem);
            var node = (ForceNode)blobItem.WrappedNode;
            var filledNode = node
                .AddForces(lists[nameof(ForceNode.Forces)].Cast<ForceNode>())
                .AddProfiles(lists[nameof(ForceNode.Profiles)].Cast<ProfileNode>())
                .AddRules(lists[nameof(ForceNode.Rules)].Cast<RuleNode>())
                .AddSelections(lists[nameof(ForceNode.Selections)].Cast<SelectionNode>());
            return filledNode;
        }

        private SelectionNode ParseSelection(GitreeNode blobItem)
        {
            var lists = ParseItemLists(blobItem);
            var node = (SelectionNode)blobItem.WrappedNode;
            var filledNode = node
                .AddProfiles(lists[nameof(SelectionNode.Profiles)].Cast<ProfileNode>())
                .AddRules(lists[nameof(SelectionNode.Rules)].Cast<RuleNode>())
                .AddSelections(lists[nameof(SelectionNode.Selections)].Cast<SelectionNode>());
            return filledNode;
        }

        private class NodeListDictionaryWithDefault
        {
            public NodeListDictionaryWithDefault(ImmutableDictionary<string, IEnumerable<SourceNode>> dictionary)
            {
                Dictionary = dictionary;
            }

            public IEnumerable<SourceNode> this[string key]
                => Dictionary.TryGetValue(key, out var value) ? value : Enumerable.Empty<SourceNode>();

            private ImmutableDictionary<string, IEnumerable<SourceNode>> Dictionary { get; }
        }
    }
}
