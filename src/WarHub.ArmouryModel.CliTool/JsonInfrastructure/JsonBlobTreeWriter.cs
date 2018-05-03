using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Optional;
using WarHub.ArmouryModel.CliTool.Utilities;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.CliTool.JsonInfrastructure
{
    /// <summary>
    /// Splits every entity into JSON with properties and folders for each collection,
    /// which contain folder for each item.
    /// </summary>
    internal class JsonBlobTreeWriter
    {
        const string Extension = ".json";
        const string ExtensionPattern = "*.json";
        public JsonBlobTreeWriter()
        {
            Serializer = ProjectSystem.JsonUtilities.CreateSerializer();
        }

        private JsonSerializer Serializer { get; }

        public string WriteItem(JsonBlobItem blobItem, DirectoryInfo directory)
        {
            INodeWithCore<NodeCore> node = blobItem.Node;
            var filename = GetFilename(blobItem);
            using (var fileStream = File.CreateText(Path.Combine(directory.FullName, filename)))
            {
                Serializer.Serialize(fileStream, node.Core);
            }
            if (blobItem.IsLeaf)
            {
                return filename;
            }
            PruneUnusedFiles(directory, new[] { filename }.ToHashSet());
            var usedNames = new HashSet<string>();
            foreach (var childList in blobItem.Children)
            {
                var dirName = WriteList(childList, directory);
                dirName.MatchSome(x => usedNames.Add(x));
            }
            PruneUnusedDirectories(directory, usedNames);
            return filename;
        }

        private Option<string> WriteList(JsonBlobList blobList, DirectoryInfo directory)
        {
            if (blobList.Items.Length == 0)
            {
                return default;
            }
            var listDirName = blobList.Name.FilenameSanitize();
            var listDir = directory.CreateSubdirectory(listDirName);
            var usedNames = new HashSet<string>();
            if (blobList.Items.All(x => x.IsLeaf))
            {
                foreach (var item in blobList.Items)
                {
                    var name = WriteItem(item, listDir);
                    usedNames.Add(name);
                }
                PruneUnusedFiles(listDir, usedNames);
            }
            else
            {
                foreach (var item in blobList.Items)
                {
                    var childDirName = item.Node.Meta.Identifier.FilenameSanitize();
                    var childDir = listDir.CreateSubdirectory(childDirName);
                    usedNames.Add(childDir.Name);
                    WriteItem(item, childDir);
                }
                PruneUnusedDirectories(listDir, usedNames);
            }
            return listDir.Name.Some();
        }

        private static void PruneUnusedFiles(DirectoryInfo directory, HashSet<string> usedNames)
        {
            foreach (var fileToRemove in
                directory.EnumerateFiles(ExtensionPattern)
                .Where(file => !usedNames.Contains(file.Name)))
            {
                fileToRemove.Delete();
            }
        }

        private static void PruneUnusedDirectories(DirectoryInfo directory, HashSet<string> usedNames)
        {
            foreach (var dirToRemove in
                directory.EnumerateDirectories()
                .Where(dir => !usedNames.Contains(dir.Name)))
            {
                dirToRemove.Delete(recursive: true);
            }
        }

        private string GetFilename(JsonBlobItem nodeFolder)
        {
            var filenameBase = nodeFolder.IsLeaf ? nodeFolder.Node.Meta.Identifier : nodeFolder.WrappedNode.Kind.ToString();
            return (filenameBase + Extension).FilenameSanitize();
        }
    }
}
