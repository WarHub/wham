using System;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.CliTool.JsonUtilities
{
    public static class SourceExtensions
    {
        public static void SwitchOnType(
            this SourceNode node,
            Action<CatalogueNode> catalogueMap = null,
            Action<CategoryNode> categoryMap = null,
            Action<CategoryEntryNode> categoryEntryMap = null,
            Action<CategoryLinkNode> categoryLinkMap = null,
            Action<CharacteristicNode> characteristicMap = null,
            Action<CharacteristicTypeNode> characteristicTypeMap = null,
            Action<ConditionNode> conditionMap = null,
            Action<ConditionGroupNode> conditionGroupMap = null,
            Action<ConstraintNode> constraintMap = null,
            Action<CostNode> costMap = null,
            Action<CostLimitNode> costLimitMap = null,
            Action<CostTypeNode> costTypeMap = null,
            Action<DatablobNode> datablobMap = null,
            Action<DataIndexNode> dataIndexMap = null,
            Action<DataIndexEntryNode> dataIndexEntryMap = null,
            Action<DataIndexRepositoryUrlNode> dataIndexRepositoryUrlMap = null,
            Action<EntryLinkNode> entryLinkMap = null,
            Action<ForceNode> forceMap = null,
            Action<ForceEntryNode> forceEntryMap = null,
            Action<GamesystemNode> gamesystemMap = null,
            Action<InfoLinkNode> infoLinkMap = null,
            Action<MetadataNode> metadataMap = null,
            Action<ModifierNode> modifierMap = null,
            Action<ProfileNode> profileMap = null,
            Action<ProfileTypeNode> profileTypeMap = null,
            Action<RepeatNode> repeatMap = null,
            Action<RosterNode> rosterMap = null,
            Action<RuleNode> ruleMap = null,
            Action<SelectionNode> selectionMap = null,
            Action<SelectionEntryNode> selectionEntryMap = null,
            Action<SelectionEntryGroupNode> selectionEntryGroupMap = null,
            Action<SourceNode> defaultAction = null)
        {
            switch (node.Kind)
            {
                case SourceKind.CostType: (costTypeMap ?? defaultAction)?.Invoke((CostTypeNode)node); break;
                case SourceKind.CharacteristicType: (characteristicTypeMap ?? defaultAction)?.Invoke((CharacteristicTypeNode)node); break;
                case SourceKind.ProfileType: (profileTypeMap ?? defaultAction)?.Invoke((ProfileTypeNode)node); break;
                case SourceKind.SelectionEntry: (selectionEntryMap ?? defaultAction)?.Invoke((SelectionEntryNode)node); break;
                case SourceKind.SelectionEntryGroup: (selectionEntryGroupMap ?? defaultAction)?.Invoke((SelectionEntryGroupNode)node); break;
                case SourceKind.CategoryEntry: (categoryEntryMap ?? defaultAction)?.Invoke((CategoryEntryNode)node); break;
                case SourceKind.ForceEntry: (forceEntryMap ?? defaultAction)?.Invoke((ForceEntryNode)node); break;
                case SourceKind.DataIndexEntry: (dataIndexEntryMap ?? defaultAction)?.Invoke((DataIndexEntryNode)node); break;
                case SourceKind.DataIndexRepositoryUrl: (dataIndexRepositoryUrlMap ?? defaultAction)?.Invoke((DataIndexRepositoryUrlNode)node); break;
                case SourceKind.Metadata: (metadataMap ?? defaultAction)?.Invoke((MetadataNode)node); break;
                case SourceKind.Condition: (conditionMap ?? defaultAction)?.Invoke((ConditionNode)node); break;
                case SourceKind.ConditionGroup: (conditionGroupMap ?? defaultAction)?.Invoke((ConditionGroupNode)node); break;
                case SourceKind.Constraint: (constraintMap ?? defaultAction)?.Invoke((ConstraintNode)node); break;
                case SourceKind.Repeat: (repeatMap ?? defaultAction)?.Invoke((RepeatNode)node); break;
                case SourceKind.Modifier: (modifierMap ?? defaultAction)?.Invoke((ModifierNode)node); break;
                case SourceKind.Cost: (costMap ?? defaultAction)?.Invoke((CostNode)node); break;
                case SourceKind.Characteristic: (characteristicMap ?? defaultAction)?.Invoke((CharacteristicNode)node); break;
                case SourceKind.Profile: (profileMap ?? defaultAction)?.Invoke((ProfileNode)node); break;
                case SourceKind.Rule: (ruleMap ?? defaultAction)?.Invoke((RuleNode)node); break;
                case SourceKind.CategoryLink: (categoryLinkMap ?? defaultAction)?.Invoke((CategoryLinkNode)node); break;
                case SourceKind.EntryLink: (entryLinkMap ?? defaultAction)?.Invoke((EntryLinkNode)node); break;
                case SourceKind.InfoLink: (infoLinkMap ?? defaultAction)?.Invoke((InfoLinkNode)node); break;
                case SourceKind.CostLimit: (costLimitMap ?? defaultAction)?.Invoke((CostLimitNode)node); break;
                case SourceKind.Category: (categoryMap ?? defaultAction)?.Invoke((CategoryNode)node); break;
                case SourceKind.Force: (forceMap ?? defaultAction)?.Invoke((ForceNode)node); break;
                case SourceKind.Selection: (selectionMap ?? defaultAction)?.Invoke((SelectionNode)node); break;
                case SourceKind.Catalogue: (catalogueMap ?? defaultAction)?.Invoke((CatalogueNode)node); break;
                case SourceKind.Gamesystem: (gamesystemMap ?? defaultAction)?.Invoke((GamesystemNode)node); break;
                case SourceKind.Roster: (rosterMap ?? defaultAction)?.Invoke((RosterNode)node); break;
                case SourceKind.DataIndex: (dataIndexMap ?? defaultAction)?.Invoke((DataIndexNode)node); break;
                case SourceKind.Datablob: (datablobMap ?? defaultAction)?.Invoke((DatablobNode)node); break;
                default:
                    defaultAction?.Invoke(node);
                    break;
            }
        }

        public static T MatchOnType<T>(
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
            Func<SourceNode, T> defaultMap = null)
        {
            defaultMap = defaultMap ?? Default;
            switch (node.Kind)
            {
                case SourceKind.CostType: return (costTypeMap ?? defaultMap).Invoke((CostTypeNode)node);
                case SourceKind.CharacteristicType: return (characteristicTypeMap ?? defaultMap).Invoke((CharacteristicTypeNode)node);
                case SourceKind.ProfileType: return (profileTypeMap ?? defaultMap).Invoke((ProfileTypeNode)node);
                case SourceKind.SelectionEntry: return (selectionEntryMap ?? defaultMap).Invoke((SelectionEntryNode)node);
                case SourceKind.SelectionEntryGroup: return (selectionEntryGroupMap ?? defaultMap).Invoke((SelectionEntryGroupNode)node);
                case SourceKind.CategoryEntry: return (categoryEntryMap ?? defaultMap).Invoke((CategoryEntryNode)node);
                case SourceKind.ForceEntry: return (forceEntryMap ?? defaultMap).Invoke((ForceEntryNode)node);
                case SourceKind.DataIndexEntry: return (dataIndexEntryMap ?? defaultMap).Invoke((DataIndexEntryNode)node);
                case SourceKind.DataIndexRepositoryUrl: return (dataIndexRepositoryUrlMap ?? defaultMap).Invoke((DataIndexRepositoryUrlNode)node);
                case SourceKind.Metadata: return (metadataMap ?? defaultMap).Invoke((MetadataNode)node);
                case SourceKind.Condition: return (conditionMap ?? defaultMap).Invoke((ConditionNode)node);
                case SourceKind.ConditionGroup: return (conditionGroupMap ?? defaultMap).Invoke((ConditionGroupNode)node);
                case SourceKind.Constraint: return (constraintMap ?? defaultMap).Invoke((ConstraintNode)node);
                case SourceKind.Repeat: return (repeatMap ?? defaultMap).Invoke((RepeatNode)node);
                case SourceKind.Modifier: return (modifierMap ?? defaultMap).Invoke((ModifierNode)node);
                case SourceKind.Cost: return (costMap ?? defaultMap).Invoke((CostNode)node);
                case SourceKind.Characteristic: return (characteristicMap ?? defaultMap).Invoke((CharacteristicNode)node);
                case SourceKind.Profile: return (profileMap ?? defaultMap).Invoke((ProfileNode)node);
                case SourceKind.Rule: return (ruleMap ?? defaultMap).Invoke((RuleNode)node);
                case SourceKind.CategoryLink: return (categoryLinkMap ?? defaultMap).Invoke((CategoryLinkNode)node);
                case SourceKind.EntryLink: return (entryLinkMap ?? defaultMap).Invoke((EntryLinkNode)node);
                case SourceKind.InfoLink: return (infoLinkMap ?? defaultMap).Invoke((InfoLinkNode)node);
                case SourceKind.CostLimit: return (costLimitMap ?? defaultMap).Invoke((CostLimitNode)node);
                case SourceKind.Category: return (categoryMap ?? defaultMap).Invoke((CategoryNode)node);
                case SourceKind.Force: return (forceMap ?? defaultMap).Invoke((ForceNode)node);
                case SourceKind.Selection: return (selectionMap ?? defaultMap).Invoke((SelectionNode)node);
                case SourceKind.Catalogue: return (catalogueMap ?? defaultMap).Invoke((CatalogueNode)node);
                case SourceKind.Gamesystem: return (gamesystemMap ?? defaultMap).Invoke((GamesystemNode)node);
                case SourceKind.Roster: return (rosterMap ?? defaultMap).Invoke((RosterNode)node);
                case SourceKind.DataIndex: return (dataIndexMap ?? defaultMap).Invoke((DataIndexNode)node);
                case SourceKind.Datablob: return (datablobMap ?? defaultMap).Invoke((DatablobNode)node);
                default:
                    return defaultMap(node);
            }
            T Default(SourceNode x) => default;
        }
    }
}
