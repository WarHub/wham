using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.CliTool.JsonUtilities
{
    public class BlobTreeToSourceRootConverter
    {
        public SourceNode ParseItem(JsonBlobItem blobItem)
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

        private ImmutableDictionary<string, IEnumerable<SourceNode>> ParseBlobLists(JsonBlobItem blobItem)
        {
            return blobItem.Children.ToImmutableDictionary(x => x.Name, ParseBlobList);
        }

        private IEnumerable<SourceNode> ParseBlobList(JsonBlobList blobList)
        {
            return blobList.Nodes.Select(ParseItem);
        }

        private CatalogueBaseNode ParseCatalogueBase(JsonBlobItem blobItem)
        {
            var lists = ParseBlobLists(blobItem);
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

        private ForceEntryNode ParseForceEntry(JsonBlobItem blobItem)
        {
            var lists = ParseBlobLists(blobItem);
            var node = (ForceEntryNode)blobItem.WrappedNode;
            var filledNode = node
                .AddForceEntries(lists[nameof(ForceEntryNode.ForceEntries)].Cast<ForceEntryNode>())
                .AddProfiles(lists[nameof(ForceEntryNode.Profiles)].Cast<ProfileNode>())
                .AddRules(lists[nameof(ForceEntryNode.Rules)].Cast<RuleNode>());
            return filledNode;
        }

        private SelectionEntryBaseNode ParseSelectionEntryBase(JsonBlobItem blobItem)
        {
            var lists = ParseBlobLists(blobItem);
            var node = (SelectionEntryBaseNode)blobItem.WrappedNode;
            var filledNode = node
                .AddProfiles(lists[nameof(SelectionEntryBaseNode.Profiles)].Cast<ProfileNode>())
                .AddRules(lists[nameof(SelectionEntryBaseNode.Rules)].Cast<RuleNode>())
                .AddSelectionEntries(lists[nameof(SelectionEntryBaseNode.SelectionEntries)].Cast<SelectionEntryNode>())
                .AddSelectionEntryGroups(lists[nameof(SelectionEntryBaseNode.SelectionEntryGroups)].Cast<SelectionEntryGroupNode>());
            return filledNode;
        }

        private RosterNode ParseRoster(JsonBlobItem blobItem)
        {
            var lists = ParseBlobLists(blobItem);
            var node = (RosterNode)blobItem.WrappedNode;
            var filledNode = node
                .AddForces(lists[nameof(RosterNode.Forces)].Cast<ForceNode>());
            return filledNode;
        }

        private ForceNode ParseForce(JsonBlobItem blobItem)
        {
            var lists = ParseBlobLists(blobItem);
            var node = (ForceNode)blobItem.WrappedNode;
            var filledNode = node
                .AddForces(lists[nameof(ForceNode.Forces)].Cast<ForceNode>())
                .AddProfiles(lists[nameof(ForceNode.Profiles)].Cast<ProfileNode>())
                .AddRules(lists[nameof(ForceNode.Rules)].Cast<RuleNode>())
                .AddSelections(lists[nameof(ForceNode.Selections)].Cast<SelectionNode>());
            return filledNode;
        }

        private SelectionNode ParseSelection(JsonBlobItem blobItem)
        {
            var lists = ParseBlobLists(blobItem);
            var node = (SelectionNode)blobItem.WrappedNode;
            var filledNode = node
                .AddProfiles(lists[nameof(SelectionNode.Profiles)].Cast<ProfileNode>())
                .AddRules(lists[nameof(SelectionNode.Rules)].Cast<RuleNode>())
                .AddSelections(lists[nameof(SelectionNode.Selections)].Cast<SelectionNode>());
            return filledNode;
        }
    }
}
