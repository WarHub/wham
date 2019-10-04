using System;
using System.Collections.Generic;
using System.Text;

namespace WarHub.ArmouryModel.Source
{
    public partial class NodeFactory
    {
        public static CatalogueNode Catalogue(string id, string name, string gameSystemId)
        {
            return Catalogue(id, name, 0, "", null, null, null, false, gameSystemId, 0);
        }

        public static CategoryEntryNode CategoryEntry(string id, string name)
        {
            return CategoryEntry(id, name, default, default, default);
        }

        public static CategoryLinkNode CategoryLink(string id, string name, string targetId)
        {
            return CategoryLink(id, name, default, default, default, targetId, default);
        }

        public static CharacteristicNode Characteristic(string id, string name)
        {
            return Characteristic(id, name, default);
        }

        public static CostNode Cost(string name, string costTypeId)
        {
            return Cost(name, costTypeId, default);
        }

        public static CostLimitNode CostLimit(string name, string costTypeId)
        {
            return CostLimit(name, costTypeId, default);
        }

        public static CostTypeNode CostType(string id, string name)
        {
            return CostType(id, name, default);
        }

        public static DatablobNode Datablob()
        {
            return Datablob(Metadata());
        }

        public static EntryLinkNode EntryLink(string id, string name, string targetId, EntryLinkKind type)
        {
            return EntryLink(id, name, default, default, default, default, default, targetId, type);
        }

        public static ForceEntryNode ForceEntry(string id, string name)
        {
            return ForceEntry(id, name, default, default, default);
        }

        public static InfoLinkNode InfoLink(string id, string name, string targetId, InfoLinkKind type)
        {
            return InfoLink(id, name, default, default, default, targetId, type);
        }

        public static MetadataNode Metadata()
        {
            return Metadata(default, default, default);
        }

        public static ProfileNode Profile(string id, string name, string profileTypeId, string profileTypeName)
        {
            return Profile(id, name, default, default, default, profileTypeId, profileTypeName);
        }

        public static RuleNode Rule(string id, string name)
        {
            return Rule(id, name, default, default, default, default);
        }

        public static RuleNode Rule(string id, string name, string description)
        {
            return Rule(id, name, default, default, default, description);
        }

        public static SelectionEntryNode SelectionEntry(string id, string name)
        {
            return SelectionEntry(id, name, default, default, default, default, default, default, SelectionEntryKind.Upgrade);
        }

        public static SelectionEntryGroupNode SelectionEntryGroup(string id, string name)
        {
            return SelectionEntryGroup(id, name, default, default, default, default, default, default);
        }
    }
}
