using System.Collections.Immutable;
using System.Linq;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    /// <summary>
    /// Reads <see cref="GitreeStorageFolderNode"/> as <see cref="GitreeNode"/>.
    /// </summary>
    internal class GitreeReader
    {
        public GitreeNode ReadItemFolder(GitreeStorageFolderNode folder)
        {
            var children = folder.GetFolders().Select(ReadListFolder).ToImmutableArray();
            var nodeDocument = folder.GetDocuments().Single();
            var (node, wrappedNode) = ReadDocumentNodes(nodeDocument);
            var blobItem = new GitreeNode(node, wrappedNode, false, children);
            return blobItem;
        }

        private GitreeListNode ReadListFolder(GitreeStorageFolderNode folder)
        {
            var documentItems = folder.GetDocuments().Select(ReadDocument).ToImmutableArray();
            var folderItems = folder.GetFolders().Select(ReadItemFolder).ToImmutableArray();
            var blobList = new GitreeListNode(folder.Name, documentItems.AddRange(folderItems));
            return blobList;
        }

        private GitreeNode ReadDocument(GitreeStorageFileNode document)
        {
            var (node, wrappedNode) = ReadDocumentNodes(document);
            var item = new GitreeNode(node, wrappedNode, true, ImmutableArray<GitreeListNode>.Empty);
            return item;
        }

        private (DatablobNode node, SourceNode wrappedNode) ReadDocumentNodes(GitreeStorageFileNode document)
        {
            var node = document.GetRoot();
            var wrappedNode = node
                .Descendants(x => x.IsList)
                .First(x => x.Kind != SourceKind.Metadata && !x.IsList);
            return (node, wrappedNode);
        }
    }
}
