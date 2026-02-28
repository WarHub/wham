using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.Gitree.Serialization;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    /// <summary>
    /// Splits every entity into Gitree with properties and folders for each collection,
    /// which contain folder for each item.
    /// </summary>
    public class GitreeWriter
    {
        private const string Extension = ".json";
        private const string ExtensionPattern = "*.json";

        private JsonSerializer Serializer { get; } = JsonUtilities.CreateSerializer();

        public string WriteItem(GitreeNode blobItem, DirectoryInfo directory)
        {
            DatablobNode node = blobItem.Datablob;
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
            foreach (var childList in blobItem.Lists)
            {
                var dirName = WriteList(childList, directory);
                if (dirName != null)
                {
                    usedNames.Add(dirName);
                }
            }
            PruneUnusedDirectories(directory, usedNames);
            return filename;
        }

        private string WriteList(GitreeListNode blobList, DirectoryInfo directory)
        {
            if (blobList.Items.Length == 0)
            {
                return null;
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
                    var childDirName = item.Datablob.Meta.Identifier.FilenameSanitize();
                    var childDir = listDir.CreateSubdirectory(childDirName);
                    usedNames.Add(childDir.Name);
                    WriteItem(item, childDir);
                }
                PruneUnusedDirectories(listDir, usedNames);
            }
            return listDir.Name;
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

        private static string GetFilename(GitreeNode nodeFolder)
        {
            var filenameBase = nodeFolder.IsLeaf ? nodeFolder.Datablob.Meta.Identifier : nodeFolder.WrappedNode.Kind.ToString();
            return (filenameBase + Extension).FilenameSanitize();
        }
    }
}
