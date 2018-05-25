using System.Collections.Immutable;
using System.Linq;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.JsonFolder;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    public class JsonTreeReader
    {
        public JsonTreeItem ReadItemFolder(JsonFolder folder)
        {
            var children = folder.GetFolders().Select(ReadListFolder).ToImmutableArray();
            var nodeDocument = folder.GetDocuments().Single();
            var (node, wrappedNode) = ReadDocumentNodes(nodeDocument);
            var blobItem = new JsonTreeItem(node, wrappedNode, false, children);
            return blobItem;
        }

        private JsonTreeItemList ReadListFolder(JsonFolder folder)
        {
            var documentItems = folder.GetDocuments().Select(ReadDocument).ToImmutableArray();
            var folderItems = folder.GetFolders().Select(ReadItemFolder).ToImmutableArray();
            var blobList = new JsonTreeItemList(folder.Name, documentItems.AddRange(folderItems));
            return blobList;
        }

        private JsonTreeItem ReadDocument(JsonDocument document)
        {
            var (node, wrappedNode) = ReadDocumentNodes(document);
            var item = new JsonTreeItem(node, wrappedNode, true, ImmutableArray<JsonTreeItemList>.Empty);
            return item;
        }

        private (DatablobNode node, SourceNode wrappedNode) ReadDocumentNodes(JsonDocument document)
        {
            var node = document.GetRoot();
            var wrappedNode = node.Children().First(x => x.Kind != SourceKind.Metadata);
            return (node, wrappedNode);
        }
    }
}
