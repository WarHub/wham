using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using WarHub.ArmouryModel.ProjectSystem;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    public class JsonWorkspace
    {
        private JsonWorkspace(DirectoryInfo directory, ProjectConfiguration projectConfiguration)
        {
            Serializer = JsonUtilities.CreateSerializer();
            Directory = directory;
            ProjectConfiguration = projectConfiguration;
            Root = new JsonFolder(directory, this);
        }

        public JsonFolder Root { get; }

        internal JsonSerializer Serializer { get; }

        private DirectoryInfo Directory { get; }

        public ProjectConfiguration ProjectConfiguration { get; }

        public static JsonWorkspace CreateFromPath(string path)
        {
            if (File.Exists(path))
            {
                return CreateFromConfigurationFile(path);
            }
            return CreateFromDirectory(path);
        }

        public static JsonWorkspace CreateFromConfigurationFile(string path)
        {
            var configProvider = new JsonFolderProjectConfigurationProvider();
            return new JsonWorkspace(new DirectoryInfo(Path.GetDirectoryName(path)), configProvider.Create(path));
        }

        public static JsonWorkspace CreateFromDirectory(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            var configFiles = dirInfo
                .EnumerateFiles("*" + ProjectConfiguration.FileExtension)
                .ToList();
            var configProvider = new JsonFolderProjectConfigurationProvider();
            switch (configFiles.Count)
            {
                case 0:
                    return new JsonWorkspace(dirInfo, configProvider.Create(path));
                case 1:
                    return new JsonWorkspace(dirInfo, configProvider.Create(configFiles[0].FullName));
                default:
                    throw new InvalidOperationException("There's more than one project file in the directory");
            }
        }
    }
}
