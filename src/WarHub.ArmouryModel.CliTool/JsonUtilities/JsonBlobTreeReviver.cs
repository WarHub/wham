using System.Collections.Immutable;
using System.Linq;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.JsonFolder;

namespace WarHub.ArmouryModel.CliTool.JsonUtilities
{
    public class JsonBlobTreeReviver
    {
        public JsonBlobItem VisitItemFolder(JsonFolder folder)
        {
            var children = folder.GetFolders().Select(VisitListFolder).ToImmutableArray();
            var nodeDocument = folder.GetDocuments().Single();
            var (node, wrappedNode) = DocumentNodes(nodeDocument);
            var blobItem = new JsonBlobItem(node, wrappedNode, false, children);
            return blobItem;
        }

        public JsonBlobList VisitListFolder(JsonFolder folder)
        {
            var documentItems = folder.GetDocuments().Select(VisitDocument).ToImmutableArray();
            var folderItems = folder.GetFolders().Select(VisitItemFolder).ToImmutableArray();
            var blobList = new JsonBlobList(folder.Name, documentItems.AddRange(folderItems));
            return blobList;
        }

        public JsonBlobItem VisitDocument(JsonDocument document)
        {
            var (node, wrappedNode) = DocumentNodes(document);
            var item = new JsonBlobItem(node, wrappedNode, true, ImmutableArray<JsonBlobList>.Empty);
            return item;
        }

        private (DatablobNode node, SourceNode wrappedNode) DocumentNodes(JsonDocument document)
        {
            var node = document.GetRoot();
            var wrappedNode = node.Children().First(x => x.Kind != SourceKind.Metadata);
            return (node, wrappedNode);
        }
    }
}
