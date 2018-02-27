using System;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.CliTool.JsonUtilities
{
    public static class SourceExtensions
    {
        public static T SwitchOnType<T>(
            this SourceNode node,
            Func<CatalogueNode, T> catalogueMap = null,
            Func<CategoryNode, T> categoryMap = null,
            Func<CategoryEntryNode, T> categoryEntryMap = null,
            Func<CategoryLinkNode, T> categoryLinkMap = null,
            Func<CharacteristicNode, T> characteristicMap = null,
            Func<CharacteristicTypeNode, T> characteristicTypeMap = null,
            Func<ConditionNode, T> conditionMap = null,
            Func<ConditionGroupNode, T> conditionGroupMap = null,
            Func<ConstraintNode, T> constraintMap = null,
            Func<CostNode, T> costMap = null,
            Func<CostLimitNode, T> costLimitMap = null,
            Func<CostTypeNode, T> costTypeMap = null,
            Func<DatablobNode, T> datablobMap = null,
            Func<DataIndexNode, T> dataIndexMap = null,
            Func<DataIndexEntryNode, T> dataIndexEntryMap = null,
            Func<DataIndexRepositoryUrlNode, T> dataIndexRepositoryUrlMap = null,
            Func<EntryLinkNode, T> entryLinkMap = null,
            Func<ForceNode, T> forceMap = null,
            Func<ForceEntryNode, T> forceEntryMap = null,
            Func<GamesystemNode, T> gamesystemMap = null,
            Func<InfoLinkNode, T> infoLinkMap = null,
            Func<MetadataNode, T> metadataMap = null,
            Func<ModifierNode, T> modifierMap = null,
            Func<ProfileNode, T> profileMap = null,
            Func<ProfileTypeNode, T> profileTypeMap = null,
            Func<RepeatNode, T> repeatMap = null,
            Func<RosterNode, T> rosterMap = null,
            Func<RuleNode, T> ruleMap = null,
            Func<SelectionNode, T> selectionMap = null,
            Func<SelectionEntryNode, T> selectionEntryMap = null,
            Func<SelectionEntryGroupNode, T> selectionEntryGroupMap = null,
            T defaultValue = default)
        {
            switch (node.Kind)
            {
                case SourceKind.CostType: return costTypeMap((CostTypeNode)node);
                case SourceKind.CharacteristicType: return characteristicTypeMap((CharacteristicTypeNode)node);
                case SourceKind.ProfileType: return profileTypeMap((ProfileTypeNode)node);
                case SourceKind.SelectionEntry: return selectionEntryMap((SelectionEntryNode)node);
                case SourceKind.SelectionEntryGroup: return selectionEntryGroupMap((SelectionEntryGroupNode)node);
                case SourceKind.CategoryEntry: return categoryEntryMap((CategoryEntryNode)node);
                case SourceKind.ForceEntry: return forceEntryMap((ForceEntryNode)node);
                case SourceKind.DataIndexEntry: return dataIndexEntryMap((DataIndexEntryNode)node);
                case SourceKind.DataIndexRepositoryUrl: return dataIndexRepositoryUrlMap((DataIndexRepositoryUrlNode)node);
                case SourceKind.Metadata: return metadataMap((MetadataNode)node);
                case SourceKind.Condition: return conditionMap((ConditionNode)node);
                case SourceKind.ConditionGroup: return conditionGroupMap((ConditionGroupNode)node);
                case SourceKind.Constraint: return constraintMap((ConstraintNode)node);
                case SourceKind.Repeat: return repeatMap((RepeatNode)node);
                case SourceKind.Modifier: return modifierMap((ModifierNode)node);
                case SourceKind.Cost: return costMap((CostNode)node);
                case SourceKind.Characteristic: return characteristicMap((CharacteristicNode)node);
                case SourceKind.Profile: return profileMap((ProfileNode)node);
                case SourceKind.Rule: return ruleMap((RuleNode)node);
                case SourceKind.CategoryLink: return categoryLinkMap((CategoryLinkNode)node);
                case SourceKind.EntryLink: return entryLinkMap((EntryLinkNode)node);
                case SourceKind.InfoLink: return infoLinkMap((InfoLinkNode)node);
                case SourceKind.CostLimit: return costLimitMap((CostLimitNode)node);
                case SourceKind.Category: return categoryMap((CategoryNode)node);
                case SourceKind.Force: return forceMap((ForceNode)node);
                case SourceKind.Selection: return selectionMap((SelectionNode)node);
                case SourceKind.Catalogue: return catalogueMap((CatalogueNode)node);
                case SourceKind.Gamesystem: return gamesystemMap((GamesystemNode)node);
                case SourceKind.Roster: return rosterMap((RosterNode)node);
                case SourceKind.DataIndex: return dataIndexMap((DataIndexNode)node);
                case SourceKind.Datablob: return datablobMap((DatablobNode)node);
                default:
                    return defaultValue;
            }
        }
    }
}
