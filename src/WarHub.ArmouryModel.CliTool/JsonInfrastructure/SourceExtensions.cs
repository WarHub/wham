using System;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.CliTool.JsonInfrastructure
{
    public static class SourceExtensions
    {
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
