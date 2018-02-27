using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WarHub.ArmouryModel.CliTool.Utilities;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.JsonFolder;

namespace WarHub.ArmouryModel.CliTool.JsonUtilities
{
    /// <summary>
    /// Splits every entity into JSON with properties and folders for each collection,
    /// which contain folder for each item.
    /// </summary>
    public class XmlToJsonWriter
    {
        public XmlToJsonWriter()
        {
            Serializer = JsonWorkspace.CreateSerializer();
        }

        private JsonSerializer Serializer { get; }

        public void WriteNode(JsonBlobItem nodeFolder, DirectoryInfo directory)
        {
            INodeWithCore<NodeCore> node = nodeFolder.Node;
            var filename = GetFilename(nodeFolder);
            using (var fileStream = File.CreateText(Path.Combine(directory.FullName, filename)))
            {
                Serializer.Serialize(fileStream, node.Core);
            }
            foreach (var childList in nodeFolder.Children)
            {
                WriteList(childList, directory);
            }
        }

        private void WriteList(JsonBlobList childList, DirectoryInfo directory)
        {
            if (childList.Nodes.Length == 0)
            {
                return;
            }
            var listDirName = childList.Name.FilenameSanitize();
            var listDir = directory.CreateSubdirectory(listDirName);
            if (childList.Nodes.All(x => x.IsLeaf))
            {
                foreach (var childNode in childList.Nodes)
                {
                    WriteNode(childNode, listDir);
                }
            }
            else
            {
                foreach (var childNode in childList.Nodes)
                {
                    var childDirName = childNode.Node.Meta.Identifier.FilenameSanitize();
                    var childDir = listDir.CreateSubdirectory(childDirName);
                    WriteNode(childNode, childDir);
                }
            }
        }

        private string GetFilename(JsonBlobItem nodeFolder)
        {
            var filenameBase = nodeFolder.IsLeaf ? nodeFolder.Node.Meta.Identifier : nodeFolder.WrappedNode.Kind.ToString();
            return (filenameBase + ".json").FilenameSanitize();
        }
    }
}
