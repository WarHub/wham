using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using MoreLinq;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.CliTool.JsonUtilities
{

    public class DatablobTreeConverter : SourceVisitor<NodeFolder>
    {
        private static DatablobNode EmptyBlob { get; } = new DatablobCore.Builder().ToImmutable().ToNode();

        public override NodeFolder DefaultVisit(SourceNode node)
        {
            var blob = BlobWith(node);
            return new NodeFolder(blob, node.Kind, ImmutableArray<ListFolder>.Empty);
        }

        public override NodeFolder VisitCatalogue(CatalogueNode node)
        {
            var result = VisitCatalogueBase(node);
            return new NodeFolder(EmptyBlob.AddCatalogues(result.node), node.Kind, result.folders);
        }

        public override NodeFolder VisitForce(ForceNode node)
        {
            var strippedLists = ImmutableHashSet.Create(
                nameof(ForceNode.Forces),
                nameof(ForceNode.Profiles),
                nameof(ForceNode.Rules),
                nameof(ForceNode.Selections));
            var listFolders = CreateListFolders(node, strippedLists);
            var strippedNode = node
                .WithForces()
                .WithProfiles()
                .WithRules()
                .WithSelections();
            return new NodeFolder(EmptyBlob.AddForces(strippedNode), node.Kind, listFolders);
        }

        public override NodeFolder VisitForceEntry(ForceEntryNode node)
        {
            var strippedLists = ImmutableHashSet.Create(
                nameof(ForceEntryNode.ForceEntries),
                nameof(ForceEntryNode.Profiles),
                nameof(ForceEntryNode.Rules));
            var listFolders = CreateListFolders(node, strippedLists);
            var strippedNode = node
                .WithForceEntries()
                .WithProfiles()
                .WithRules();
            return new NodeFolder(EmptyBlob.AddForceEntries(strippedNode), node.Kind, listFolders);
        }

        public override NodeFolder VisitGamesystem(GamesystemNode node)
        {
            var result = VisitCatalogueBase(node);
            return new NodeFolder(EmptyBlob.AddGamesystems(result.node), node.Kind, result.folders);
        }

        public override NodeFolder VisitRoster(RosterNode node)
        {
            var strippedLists = ImmutableHashSet.Create(nameof(RosterNode.Forces));
            var listFolders = CreateListFolders(node, strippedLists);
            var strippedNode = node.WithForces();
            return new NodeFolder(EmptyBlob.AddRosters(strippedNode), node.Kind, listFolders);
        }

        public override NodeFolder VisitSelection(SelectionNode node)
        {
            var strippedLists = ImmutableHashSet.Create(
                nameof(SelectionNode.Profiles),
                nameof(SelectionNode.Rules),
                nameof(SelectionNode.Selections));
            var listFolders = CreateListFolders(node, strippedLists);
            var strippedNode = node
                .WithProfiles()
                .WithRules()
                .WithSelections();
            return new NodeFolder(EmptyBlob.AddSelections(strippedNode), node.Kind, listFolders);
        }

        public override NodeFolder VisitSelectionEntry(SelectionEntryNode node)
        {
            var result = VisitSelectionEntryBase(node);
            return new NodeFolder(EmptyBlob.AddSelectionEntries(result.node), node.Kind, result.folders);
        }

        public override NodeFolder VisitSelectionEntryGroup(SelectionEntryGroupNode node)
        {
            var result = VisitSelectionEntryBase(node);
            return new NodeFolder(EmptyBlob.AddSelectionEntryGroups(result.node), node.Kind, result.folders);
        }

        private (T node, ImmutableArray<ListFolder> folders) VisitCatalogueBase<T>(T node) where T : CatalogueBaseNode
        {
            var strippedLists = ImmutableHashSet.Create(
                nameof(CatalogueBaseNode.ForceEntries),
                nameof(CatalogueBaseNode.Profiles),
                nameof(CatalogueBaseNode.ProfileTypes),
                nameof(CatalogueBaseNode.Rules),
                nameof(CatalogueBaseNode.SelectionEntries),
                nameof(CatalogueBaseNode.SharedProfiles),
                nameof(CatalogueBaseNode.SharedRules),
                nameof(CatalogueBaseNode.SharedSelectionEntries),
                nameof(CatalogueBaseNode.SharedSelectionEntryGroups));
            var listFolders = CreateListFolders(node, strippedLists);
            var strippedNode = node
                .WithForceEntries()
                .WithProfiles()
                .WithProfileTypes()
                .WithRules()
                .WithSelectionEntries()
                .WithSharedProfiles()
                .WithSharedRules()
                .WithSharedSelectionEntries()
                .WithSharedSelectionEntryGroups();
            return ((T)strippedNode, listFolders);
        }

        private (T node, ImmutableArray<ListFolder> folders) VisitSelectionEntryBase<T>(T node) where T : SelectionEntryBaseNode
        {
            var strippedLists = ImmutableHashSet.Create(
                nameof(SelectionEntryBaseNode.Profiles),
                nameof(SelectionEntryBaseNode.Rules),
                nameof(SelectionEntryBaseNode.SelectionEntries),
                nameof(SelectionEntryBaseNode.SelectionEntryGroups));
            var listFolders = CreateListFolders(node, strippedLists);
            var strippedNode = node
                .WithProfiles()
                .WithRules()
                .WithSelectionEntries()
                .WithSelectionEntryGroups();
            return ((T)strippedNode, listFolders);
        }

        private ImmutableArray<ListFolder> CreateListFolders<TNode>(TNode node, ImmutableHashSet<string> listNames)
            where TNode : SourceNode
        {

            var listFolders = node
                .NamedChildrenLists()
                .Where(l => listNames.Contains(l.Name))
                .Select(CreateListFolder)
                .ToImmutableArray();
            return listFolders;
        }

        private ListFolder CreateListFolder(NamedNodeOrList arg)
        {
            var names = arg.ToImmutableDictionary(x => x, x => x is INameableNode named ? named.Name : "");
            var nameCounts = names.CountBy(x => x.Value).ToImmutableDictionary();
            var folders = arg
                .Select(Visit)
                .Select(AssignIdentifier)
                .Lag(1, null, AssignIdentifierPrevious)
                .ToImmutableArray();
            return new ListFolder(arg.Name, folders);

            NodeFolder AssignIdentifier(NodeFolder node, int index)
            {
                var currNode = node.Node;
                var name = names[currNode];
                var identifier = nameCounts[name] == 1 ? name : $"{name} {index}";
                return node.WithNode(currNode.WithMeta(currNode.Meta.WithIdentifier(identifier)));
            }
            NodeFolder AssignIdentifierPrevious(NodeFolder folder, NodeFolder previous)
            {
                if (previous == null)
                {
                    return folder;
                }
                return folder.WithNode(folder.Node.WithMeta(folder.Node.Meta.WithIdentifier(previous.Node.Meta.Identifier)));
            }
        }

        public static DatablobNode BlobWith(SourceNode node)
        {
            return node.SwitchOnType(
                x => EmptyBlob.AddCatalogues(x),
                x => EmptyBlob.AddCategories(x),
                x => EmptyBlob.AddCategoryEntries(x),
                x => EmptyBlob.AddCategoryLinks(x),
                x => EmptyBlob.AddCharacteristics(x),
                x => EmptyBlob.AddCharacteristicTypes(x),
                x => EmptyBlob.AddConditions(x),
                x => EmptyBlob.AddConditionGroups(x),
                x => EmptyBlob.AddConstraints(x),
                x => EmptyBlob.AddCosts(x),
                x => EmptyBlob.AddCostLimits(x),
                x => EmptyBlob.AddCostTypes(x),
                x => EmptyBlob.AddDatablobs(x),
                x => EmptyBlob.AddDataIndexes(x),
                x => EmptyBlob.AddDataIndexEntries(x),
                x => EmptyBlob,
                x => EmptyBlob.AddEntryLinks(x),
                x => EmptyBlob.AddForces(x),
                x => EmptyBlob.AddForceEntries(x),
                x => EmptyBlob.AddGamesystems(x),
                x => EmptyBlob.AddInfoLinks(x),
                x => EmptyBlob,
                x => EmptyBlob.AddModifiers(x),
                x => EmptyBlob.AddProfiles(x),
                x => EmptyBlob.AddProfileTypes(x),
                x => EmptyBlob.AddRepeats(x),
                x => EmptyBlob.AddRosters(x),
                x => EmptyBlob.AddRules(x),
                x => EmptyBlob.AddSelections(x),
                x => EmptyBlob.AddSelectionEntries(x),
                x => EmptyBlob.AddSelectionEntryGroups(x),
                EmptyBlob);
        }
    }
}
