using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source
{
    public static partial class NodeFactory
    {
        private static class Defaults
        {
            public const string Field = "selections";
            public const string Scope = "parent";
            public const string ChildId = "model";
            public const string ModifierField = "name";
            public const string ModifierValue = "0";
            public const int Revision = 1;
            public const decimal SelectorValue = 1m;
        }

        /// <summary>
        /// Creates a new UUID using <see cref="Guid.NewGuid()"/> and cropping
        /// the resulting string representation to format "xxxx-xxxx-xxxx-xxxx"
        /// (4 groups of 4 characters).
        /// </summary>
        /// <returns>Generated short format UUID.</returns>
        private static string NewId() => Guid.NewGuid().ToString().Substring(4, 19);

        /// <summary>
        /// Generates new name via prepending caller name with "New ".
        /// </summary>
        private static string NewName([CallerMemberName]string? callerMember = null)
            => "New " + callerMember;

        public static CatalogueNode Catalogue(GamesystemNode gamesystem, string? name = null, string? id = null)
        {
            return Catalogue(
                comment: null,
                id: id ?? NewId(),
                name: name ?? NewName(),
                revision: Defaults.Revision,
                battleScribeVersion: RootElement.Catalogue.Info().CurrentVersion.BattleScribeString,
                authorName: null,
                authorContact: null,
                authorUrl: null,
                isLibrary: false,
                gamesystemId: gamesystem.Id,
                gamesystemRevision: gamesystem.Revision);
        }

        public static CatalogueLinkNode CatalogueLink(CatalogueNode catalogue, string? id = null)
        {
            return CatalogueLink(
                comment: null,
                id: id ?? NewId(),
                name: catalogue.Name,
                targetId: catalogue.Id,
                type: CatalogueLinkKind.Catalogue,
                importRootEntries: true);
        }

        public static CategoryNode Category(CategoryEntryNode categoryEntry, string? id = null)
        {
            return Category(
                id: id ?? NewId(),
                categoryEntry.Name,
                categoryEntry.Id,
                entryGroupId: null,
                categoryEntry.PublicationId,
                categoryEntry.Page,
                primary: false);
        }

        public static CategoryEntryNode CategoryEntry(string? name = null, string? id = null)
        {
            return CategoryEntry(
                comment: null,
                id: id ?? NewId(),
                name: name ?? NewName(),
                publicationId: null,
                page: null,
                hidden: false);
        }

        public static CategoryLinkNode CategoryLink(CategoryEntryNode categoryEntry, string? id = null)
        {
            return CategoryLink(
                comment: null,
                id: id ?? NewId(),
                name: categoryEntry.Name,
                publicationId: categoryEntry.PublicationId,
                page: categoryEntry.Page,
                hidden: false,
                targetId: categoryEntry.Id,
                primary: false);
        }

        public static CharacteristicNode Characteristic(CharacteristicTypeNode characteristicType, string? value = null)
        {
            return Characteristic(
                name: characteristicType.Name,
                typeId: characteristicType.Id,
                value: value);
        }

        public static CharacteristicTypeNode CharacteristicType(string? name = null)
        {
            return CharacteristicType(
                id: NewId(),
                name: name ?? NewName());
        }

        public static ConditionNode Condition(
            string field = Defaults.Field,
            string scope = Defaults.Scope,
            decimal value = Defaults.SelectorValue,
            string childId = Defaults.ChildId,
            ConditionKind type = ConditionKind.EqualTo)
        {
            return Condition(
                comment: null,
                field: field,
                scope: scope,
                value: value,
                isValuePercentage: false,
                shared: true,
                includeChildSelections: false,
                includeChildForces: false,
                childId: childId,
                type: type);
        }

        public static ConditionGroupNode ConditionGroup(
            ConditionGroupKind type = ConditionGroupKind.And)
        {
            return ConditionGroup(
                comment: null,
                type: type);
        }

        public static ConstraintNode Constraint(
            string field = Defaults.Field,
            string scope = Defaults.Scope,
            decimal value = Defaults.SelectorValue,
            string? id = null,
            ConstraintKind type = ConstraintKind.Minimum)
        {
            return Constraint(
                comment: null,
                field: field,
                scope: scope,
                value: value,
                isValuePercentage: false,
                shared: true,
                includeChildSelections: false,
                includeChildForces: false,
                id: id ?? NewId(),
                type: type);
        }

        public static CostNode Cost(CostTypeNode costType, decimal value = 0m)
        {
            return Cost(
                name: costType.Name,
                typeId: costType.Id,
                value: value);
        }

        public static CostLimitNode CostLimit(CostTypeNode costType, decimal? value = null)
        {
            return CostLimit(
                name: costType.Name,
                typeId: costType.Id,
                value: value ?? costType.DefaultCostLimit);
        }

        public static CostTypeNode CostType(string? name = null, decimal defaultCostLimit = -1m)
        {
            return CostType(
                comment: null,
                id: NewId(),
                name: name ?? NewName(),
                defaultCostLimit: defaultCostLimit);
        }

        public static DatablobNode Datablob()
        {
            return Datablob(
                meta: Metadata());
        }

        public static DataIndexNode DataIndex(string? name = null, string? indexUrl = null)
        {
            return DataIndex(
                battleScribeVersion: RootElement.DataIndex.Info().CurrentVersion.BattleScribeString,
                name: name ?? NewName(),
                indexUrl: indexUrl);
        }

        public static DataIndexEntryNode DataIndexEntry(string filePath, CatalogueNode node)
        {
            return DataIndexEntry(filePath, node, DataIndexEntryKind.Catalogue);
        }

        public static DataIndexEntryNode DataIndexEntry(string filePath, GamesystemNode node)
        {
            return DataIndexEntry(filePath, node, DataIndexEntryKind.Gamesystem);
        }

        private static DataIndexEntryNode DataIndexEntry(string filePath, CatalogueBaseNode node, DataIndexEntryKind dataType)
        {
            return DataIndexEntry(
                filePath: filePath,
                dataType: dataType,
                dataId: node.Id,
                dataName: node.Name,
                dataBattleScribeVersion: node.BattleScribeVersion,
                dataRevision: node.Revision);
        }

        public static EntryLinkNode EntryLink(SelectionEntryNode selectionEntry, string? id = null)
        {
            return EntryLink(
                selectionEntryBase: selectionEntry,
                type: EntryLinkKind.SelectionEntry,
                id: id);
        }

        public static EntryLinkNode EntryLink(SelectionEntryGroupNode selectionEntryGroup, string? id = null)
        {
            return EntryLink(
                selectionEntryBase: selectionEntryGroup,
                type: EntryLinkKind.SelectionEntryGroup,
                id: id);
        }

        private static EntryLinkNode EntryLink(SelectionEntryBaseNode selectionEntryBase, EntryLinkKind type, string? id = null)
        {
            return EntryLink(
                comment: null,
                id: id ?? NewId(),
                name: selectionEntryBase.Name,
                publicationId: selectionEntryBase.PublicationId,
                page: selectionEntryBase.Page,
                hidden: false,
                collective: false,
                exported: true,
                targetId: selectionEntryBase.Id,
                type: type);
        }

        public static ForceNode Force(ForceEntryNode forceEntry, string? id = null)
        {
            var catalogue = forceEntry.FirstAncestorOrSelf<CatalogueBaseNode>();
            if (catalogue is null)
            {
                throw new ArgumentException(
                    "Can't use ForceEntry that isn't a descendant of any Catalogue or Gamesystem.",
                    nameof(forceEntry));
            }
            return Force(
                id: id ?? NewId(),
                name: forceEntry.Name,
                entryId: forceEntry.Id,
                entryGroupId: null,
                publicationId: forceEntry.PublicationId,
                page: forceEntry.Page,
                catalogueId: catalogue.Id,
                catalogueRevision: catalogue.Revision,
                catalogueName: catalogue.Name);
        }

        public static ForceEntryNode ForceEntry(string? name = null, string? id = null)
        {
            return ForceEntry(
                comment: null,
                id: id ?? NewId(),
                name: name ?? NewName(),
                publicationId: null,
                page: null,
                hidden: false);
        }

        public static GamesystemNode Gamesystem(string? name = null, string? id = null)
        {
            return Gamesystem(
                comment: null,
                id: id ?? NewId(),
                name: name ?? NewName(),
                revision: Defaults.Revision,
                battleScribeVersion: RootElement.GameSystem.Info().CurrentVersion.BattleScribeString,
                authorName: null,
                authorContact: null,
                authorUrl: null);
        }

        public static InfoGroupNode InfoGroup(string? name = null, string? id = null)
        {
            return InfoGroup(
                comment: null,
                id: id ?? NewId(),
                name: name ?? NewName(),
                publicationId: null,
                page: null,
                hidden: false);
        }

        public static InfoLinkNode InfoLink(InfoGroupNode infoGroup, string? id = null)
        {
            return InfoLink(infoGroup, InfoLinkKind.InfoGroup, id);
        }

        public static InfoLinkNode InfoLink(ProfileNode profile, string? id = null)
        {
            return InfoLink(profile, InfoLinkKind.Profile, id);
        }

        public static InfoLinkNode InfoLink(RuleNode rule, string? id = null)
        {
            return InfoLink(rule, InfoLinkKind.Rule, id);
        }

        private static InfoLinkNode InfoLink(EntryBaseNode node, InfoLinkKind type, string? id = null)
        {
            return InfoLink(
                comment: null,
                id: id ?? NewId(),
                name: node.Name,
                publicationId: node.PublicationId,
                page: node.Page,
                hidden: false,
                targetId: node.Id,
                type: type);
        }

        public static NodeList<T> SingletonList<T>(T node)
            where T : SourceNode
        {
            return NodeList.Create(node);
        }

        public static NodeList<T> List<T>(IEnumerable<T> nodes)
            where T : SourceNode
        {
            return NodeList.Create(nodes);
        }

        public static NodeList<T> List<T>(params T[] nodes)
            where T : SourceNode
        {
            return NodeList.Create(nodes);
        }

        public static MetadataNode Metadata()
        {
            return Metadata(
                identifier: default,
                prevIdentifier: default,
                sequence: default);
        }

        public static ModifierNode Modifier(
            ModifierKind type = ModifierKind.Set,
            string field = Defaults.ModifierField)
        {
            return Modifier(
                comment: null,
                type: type,
                field: field,
                value: Defaults.ModifierValue);
        }

        public static ModifierGroupNode ModifierGroup()
        {
            return ModifierGroup(
                comment: null);
        }

        public static ProfileNode Profile(ProfileTypeNode profileType, string? name = null, string? id = null)
        {
            return Profile(
                comment: null,
                id: id ?? NewId(),
                name: name ?? NewName(),
                publicationId: null,
                page: null,
                hidden: false,
                typeId: profileType.Id,
                typeName: profileType.Name);
        }

        public static ProfileTypeNode ProfileType(string? name = null)
        {
            return ProfileType(
                comment: null,
                id: NewId(),
                name: name ?? NewName());
        }

        public static PublicationNode Publication(string? name = null)
        {
            return Publication(
                comment: null,
                id: NewId(),
                name: name ?? NewName());
        }

        public static RepeatNode Repeat(
            string field = Defaults.Field,
            string scope = Defaults.Scope,
            decimal value = Defaults.SelectorValue,
            string childId = Defaults.ChildId)
        {
            return Repeat(
                comment: null,
                field: field,
                scope: scope,
                value: value,
                isValuePercentage: false,
                shared: true,
                includeChildSelections: false,
                includeChildForces: false,
                childId: childId,
                repeatCount: 1,
                roundUp: false);
        }

        public static RosterNode Roster(GamesystemNode gamesystem, string? name = null, string? id = null)
        {
            return Roster(
                id: id ?? NewId(),
                name: name ?? NewName(),
                battleScribeVersion: RootElement.Roster.Info().CurrentVersion.BattleScribeString,
                gameSystemId: gamesystem.Id,
                gameSystemName: gamesystem.Name,
                gameSystemRevision: gamesystem.Revision);
        }

        public static RuleNode Rule(string? name = null, string? id = null, string? description = null)
        {
            return Rule(
                comment: null,
                id: id ?? NewId(),
                name: name ?? NewName(),
                publicationId: null,
                page: null,
                hidden: false,
                description: description);
        }

        public static SelectionNode Selection(
            SelectionEntryNode selectionEntry,
            string entryId,
            string? entryGroupId = null,
            string? id = null)
        {
            return Selection(
                id: id ?? NewId(),
                name: selectionEntry.Name,
                entryId: entryId,
                entryGroupId: entryGroupId,
                publicationId: selectionEntry.PublicationId,
                page: selectionEntry.Page,
                number: 1,
                type: selectionEntry.Type);
        }

        public static SelectionEntryNode SelectionEntry(string? name = null, string? id = null)
        {
            return SelectionEntry(
                comment: null,
                id: id ?? NewId(),
                name: name ?? NewName(),
                publicationId: null,
                page: null,
                hidden: false,
                collective: false,
                exported: true,
                type: SelectionEntryKind.Upgrade);
        }

        public static SelectionEntryGroupNode SelectionEntryGroup(string? name = null, string? id = null)
        {
            return SelectionEntryGroup(
                comment: null,
                id: id ?? NewId(),
                name: name ?? NewName(),
                publicationId: null,
                page: null,
                hidden: false,
                collective: false,
                exported: true,
                defaultSelectionEntryId: null);
        }
    }
}
