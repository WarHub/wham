using System.IO;
using Newtonsoft.Json;
using WarHub.ArmouryModel.CliTool.Utilities;
using WarHub.ArmouryModel.Source;

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
            Serializer = new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new IgnoringEmptyCollectionsContractResolver(),
                Converters = { new MultiLineStringConverter() }
            };
        }

        private JsonSerializer Serializer { get; }

        public void WriteNode(NodeFolder nodeFolder, DirectoryInfo directory)
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

        private void WriteList(ListFolder childList, DirectoryInfo directory)
        {
            if (childList.Nodes.Length == 0)
            {
                return;
            }
            var listDirName = childList.Name.FilenameSanitize();
            var listDir = directory.CreateSubdirectory(listDirName);
            foreach (var childNode in childList.Nodes)
            {
                var childDirName = childNode.Node.Meta.Identifier.FilenameSanitize();
                var childDir = listDir.CreateSubdirectory(childDirName);
                WriteNode(childNode, childDir);
            }
        }

        private string GetFilename(NodeFolder nodeFolder)
        {
            var filename = nodeFolder.WrappedNode.Kind.ToString() + ".json";
            return filename.FilenameSanitize();
        }
    }
}
