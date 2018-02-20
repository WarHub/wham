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
            switch (node)
            {
                case CatalogueNode catalogue: return catalogueMap(catalogue);
                case CategoryNode category: return categoryMap(category);
                case CategoryEntryNode categoryEntry: return categoryEntryMap(categoryEntry);
                case CategoryLinkNode categoryLink: return categoryLinkMap(categoryLink);
                case CharacteristicNode characteristic: return characteristicMap(characteristic);
                case CharacteristicTypeNode characteristicType: return characteristicTypeMap(characteristicType);
                case ConditionNode condition: return conditionMap(condition);
                case ConditionGroupNode conditionGroup: return conditionGroupMap(conditionGroup);
                case ConstraintNode constraint: return constraintMap(constraint);
                case CostNode cost: return costMap(cost);
                case CostLimitNode costLimit: return costLimitMap(costLimit);
                case CostTypeNode costType: return costTypeMap(costType);
                case DatablobNode datablob: return datablobMap(datablob);
                case DataIndexNode dataIndex: return dataIndexMap(dataIndex);
                case DataIndexEntryNode dataIndexEntry: return dataIndexEntryMap(dataIndexEntry);
                case DataIndexRepositoryUrlNode dataIndexRepositoryUrl: return dataIndexRepositoryUrlMap(dataIndexRepositoryUrl);
                case EntryLinkNode entryLink: return entryLinkMap(entryLink);
                case ForceNode force: return forceMap(force);
                case ForceEntryNode forceEntry: return forceEntryMap(forceEntry);
                case GamesystemNode gamesystem: return gamesystemMap(gamesystem);
                case InfoLinkNode infoLink: return infoLinkMap(infoLink);
                case MetadataNode metadata: return metadataMap(metadata);
                case ModifierNode modifier: return modifierMap(modifier);
                case ProfileNode profile: return profileMap(profile);
                case ProfileTypeNode profileType: return profileTypeMap(profileType);
                case RepeatNode repeat: return repeatMap(repeat);
                case RosterNode roster: return rosterMap(roster);
                case RuleNode rule: return ruleMap(rule);
                case SelectionNode selection: return selectionMap(selection);
                case SelectionEntryNode selectionEntry: return selectionEntryMap(selectionEntry);
                case SelectionEntryGroupNode selectionEntryGroup: return selectionEntryGroupMap(selectionEntryGroup);
                default:
                    return defaultValue;
            }
        }
    }
}
