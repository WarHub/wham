using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using MoreLinq;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.CliTool.JsonUtilities
{

    public class NodeToJsonBlobTreeConverter : SourceVisitor<JsonBlobItem>
    {
        private static DatablobNode EmptyBlob { get; }
            = new DatablobCore.Builder { Meta = new MetadataCore.Builder() }.ToImmutable().ToNode();

        public override JsonBlobItem DefaultVisit(SourceNode node)
        {
            var blob = BlobWith(node);
            return new JsonBlobItem(blob, node, IsLeaf: true, ImmutableArray<JsonBlobList>.Empty);
        }

        public override JsonBlobItem VisitCatalogue(CatalogueNode node)
        {
            var result = VisitCatalogueBase(node);
            return new JsonBlobItem(EmptyBlob.AddCatalogues(result.node), node, IsLeaf: false, result.folders);
        }

        public override JsonBlobItem VisitForce(ForceNode node)
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
            return new JsonBlobItem(EmptyBlob.AddForces(strippedNode), node, IsLeaf: false, listFolders);
        }

        public override JsonBlobItem VisitForceEntry(ForceEntryNode node)
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
            return new JsonBlobItem(EmptyBlob.AddForceEntries(strippedNode), node, IsLeaf: false, listFolders);
        }

        public override JsonBlobItem VisitGamesystem(GamesystemNode node)
        {
            var result = VisitCatalogueBase(node);
            return new JsonBlobItem(EmptyBlob.AddGamesystems(result.node), node, IsLeaf: false, result.folders);
        }

        public override JsonBlobItem VisitRoster(RosterNode node)
        {
            var strippedLists = ImmutableHashSet.Create(nameof(RosterNode.Forces));
            var listFolders = CreateListFolders(node, strippedLists);
            var strippedNode = node.WithForces();
            return new JsonBlobItem(EmptyBlob.AddRosters(strippedNode), node, IsLeaf: false, listFolders);
        }

        public override JsonBlobItem VisitSelection(SelectionNode node)
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
            return new JsonBlobItem(EmptyBlob.AddSelections(strippedNode), node, IsLeaf: false, listFolders);
        }

        public override JsonBlobItem VisitSelectionEntry(SelectionEntryNode node)
        {
            var result = VisitSelectionEntryBase(node);
            return new JsonBlobItem(EmptyBlob.AddSelectionEntries(result.node), node, IsLeaf: false, result.folders);
        }

        public override JsonBlobItem VisitSelectionEntryGroup(SelectionEntryGroupNode node)
        {
            var result = VisitSelectionEntryBase(node);
            return new JsonBlobItem(EmptyBlob.AddSelectionEntryGroups(result.node), node, IsLeaf: false, result.folders);
        }

        private (T node, ImmutableArray<JsonBlobList> folders) VisitCatalogueBase<T>(T node) where T : CatalogueBaseNode
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

        private (T node, ImmutableArray<JsonBlobList> folders) VisitSelectionEntryBase<T>(T node) where T : SelectionEntryBaseNode
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

        private ImmutableArray<JsonBlobList> CreateListFolders<TNode>(TNode node, ImmutableHashSet<string> listNames)
            where TNode : SourceNode
        {

            var listFolders = node
                .NamedChildrenLists()
                .Where(l => listNames.Contains(l.Name))
                .Select(CreateListFolder)
                .ToImmutableArray();
            return listFolders;
        }

        private JsonBlobList CreateListFolder(NamedNodeOrList nodeOrList)
        {
            var names = nodeOrList.ToImmutableDictionary(x => x, SelectName);
            var nameCounts = names.CountBy(x => x.Value).ToImmutableDictionary();
            var folders = nodeOrList
                .Select(Visit)
                .Select(AssignIdentifier)
                .Lag(1, null, AssignIdentifierPrevious)
                .ToImmutableArray();
            return new JsonBlobList(nodeOrList.Name, folders);

            JsonBlobItem AssignIdentifier(JsonBlobItem nodeFolder, int index)
            {
                var currNode = nodeFolder.Node;
                var name = names[nodeFolder.WrappedNode];
                var identifier = nameCounts[name] == 1 ? name : $"{name} - {index}";
                return nodeFolder.WithNode(currNode.WithMeta(currNode.Meta.WithIdentifier(identifier)));
            }
            JsonBlobItem AssignIdentifierPrevious(JsonBlobItem folder, JsonBlobItem previous)
            {
                if (previous == null)
                {
                    return folder;
                }
                var node = folder.Node;
                var newMeta = node.Meta.WithPrevIdentifier(previous.Node.Meta.Identifier);
                return folder.WithNode(node.WithMeta(newMeta));
            }
        }

        public static string SelectName(SourceNode node)
        {
            return node is INameableNode named ? named.Name : node.Kind.ToString();
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
