using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source
{
    public partial class NodeFactory
    {
        public static CatalogueNode Catalogue(string id, string name, string gamesystemId)
        {
            return Catalogue(
                id,
                name,
                revision: 1,
                battleScribeVersion: RootElement.Catalogue.Info().CurrentVersion.BattleScribeString,
                authorName: null,
                authorContact: null,
                authorUrl: null,
                isLibrary: false,
                gamesystemId,
                gamesystemRevision: 1);
        }

        public static CatalogueLinkNode CatalogueLink(string id, string name, string targetId)
        {
            return CatalogueLink(
                id,
                name,
                targetId,
                type: CatalogueLinkKind.Catalogue,
                importRootEntries: true);
        }

        public static CategoryNode Category(string id, string name, string entryId)
        {
            return Category(
                id,
                name,
                entryId,
                entryGroupId: default,
                publicationId: null,
                page: null,
                isPrimary: false);
        }

        public static CategoryEntryNode CategoryEntry(string id, string name)
        {
            return CategoryEntry(
                id,
                name,
                publicationId: null,
                page: null,
                isHidden: false);
        }

        public static CategoryLinkNode CategoryLink(string id, string name, string targetId)
        {
            return CategoryLink(
                id,
                name,
                publicationId: null,
                page: null,
                isHidden: false,
                targetId,
                isPrimary: false);
        }

        public static CharacteristicNode Characteristic(string id, string name)
        {
            return Characteristic(id, name, value: default);
        }

        public static ConstraintNode Constraint(string field, string scope, string id)
        {
            return Constraint(
                field,
                scope,
                value: -1m,
                percentValue: false,
                shared: true,
                includeChildSelections: false,
                includeChildForces: false,
                id,
                type: ConstraintKind.Maximum);
        }

        public static CostNode Cost(string name, string costTypeId)
        {
            return Cost(name, costTypeId, value: 0m);
        }

        public static CostLimitNode CostLimit(string name, string costTypeId)
        {
            return CostLimit(name, costTypeId, value: -1m);
        }

        public static CostTypeNode CostType(string id, string name)
        {
            return CostType(id, name, defaultCostLimit: -1m);
        }

        public static DatablobNode Datablob()
        {
            return Datablob(Metadata());
        }

        public static DataIndexNode DataIndex(string name)
        {
            return DataIndex(
                battleScribeVersion: RootElement.DataIndex.Info().CurrentVersion.BattleScribeString,
                name,
                indexUrl: default);
        }

        public static DataIndexEntryNode DataIndexEntry(string filePath, CatalogueNode node)
        {
            return DataIndexEntry(filePath, DataIndexEntryKind.Catalogue, node);
        }

        public static DataIndexEntryNode DataIndexEntry(string filePath, GamesystemNode node)
        {
            return DataIndexEntry(filePath, DataIndexEntryKind.Gamesystem, node);
        }

        private static DataIndexEntryNode DataIndexEntry(string filePath, DataIndexEntryKind dataType, CatalogueBaseNode node)
        {
            return DataIndexEntry(filePath, dataType, node.Id, node.Name, node.BattleScribeVersion, node.Revision);
        }

        public static EntryLinkNode EntryLink(string id, string name, string targetId, EntryLinkKind type)
        {
            return EntryLink(
                id,
                name,
                publicationId: null,
                page: null,
                isHidden: false,
                collective: false,
                import: true,
                targetId,
                type);
        }

        public static ForceNode Force(string id, string name, string entryId, string catalogueId, int catalogueRevision, string catalogueName)
        {
            return Force(
                id,
                name,
                entryId,
                entryGroupId: null,
                publicationId: null,
                page: null,
                catalogueId,
                catalogueRevision,
                catalogueName);
        }

        public static ForceEntryNode ForceEntry(string id, string name)
        {
            return ForceEntry(
                id,
                name,
                publicationId: null,
                page: null,
                isHidden: false);
        }

        public static GamesystemNode Gamesystem(string id, string name)
        {
            return Gamesystem(
                id,
                name,
                revision: 1,
                battleScribeVersion: RootElement.GameSystem.Info().CurrentVersion.BattleScribeString,
                authorName: null,
                authorContact: null,
                authorUrl: null);
        }

        public static InfoGroupNode InfoGroup(string id, string name)
        {
            return InfoGroup(
                id,
                name,
                publicationId: null,
                page: null,
                isHidden: false);
        }

        public static InfoLinkNode InfoLink(string id, string name, string targetId, InfoLinkKind type)
        {
            return InfoLink(
                id,
                name,
                publicationId: null,
                page: null,
                isHidden: false,
                targetId,
                type);
        }

        public static MetadataNode Metadata()
        {
            return Metadata(identifier: default, prevIdentifier: default, sequence: default);
        }

        public static ModifierNode Modifier(string field)
        {
            return Modifier(ModifierKind.Set, field, value: default);
        }

        public static ProfileNode Profile(string id, string name, string profileTypeId, string profileTypeName)
        {
            return Profile(
                id,
                name,
                publicationId: null,
                page: null,
                isHidden: false,
                profileTypeId,
                profileTypeName);
        }

        public static RepeatNode Repeat(string field, string scope, string childId)
        {
            return Repeat(
                field,
                scope,
                value: 1,
                percentValue: false,
                shared: true,
                includeChildSelections: false,
                includeChildForces: false,
                childId,
                repeats: 1,
                isRoundUp: false);
        }

        public static RosterNode Roster(string id, string name)
        {
            return Roster(
                id,
                name,
                battleScribeVersion: RootElement.Roster.Info().CurrentVersion.BattleScribeString,
                gameSystemId: default,
                gameSystemName: default,
                gameSystemRevision: default);
        }

        public static RuleNode Rule(string id, string name)
        {
            return Rule(id, name, description: null);
        }

        public static RuleNode Rule(string id, string name, string description)
        {
            return Rule(
                id,
                name,
                publicationId: null,
                page: null,
                isHidden: false,
                description);
        }

        public static SelectionNode Selection(string id, string name, string entryId, string entryGroupId, SelectionEntryKind type)
        {
            return Selection(
                id,
                name,
                entryId,
                entryGroupId,
                publicationId: null,
                page: null,
                number: 1,
                type);
        }

        public static SelectionEntryNode SelectionEntry(string id, string name)
        {
            return SelectionEntry(
                id,
                name,
                publicationId: null,
                page: null,
                isHidden: false,
                collective: false,
                import: true,
                categoryEntryId: default,
                SelectionEntryKind.Upgrade);
        }

        public static SelectionEntryGroupNode SelectionEntryGroup(string id, string name)
        {
            return SelectionEntryGroup(
                id,
                name,
                publicationId: null,
                page: null,
                isHidden: false,
                collective: false,
                import: true,
                defaultSelectionEntryId: default);
        }
    }
}
