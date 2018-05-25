using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using WarHub.ArmouryModel.ProjectModel;
using MoreLinq;
using Optional;
using Optional.Collections;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{

    public class JsonWorkspace : IWorkspace
    {
        private JsonWorkspace(DirectoryInfo directory, ProjectConfiguration projectConfiguration)
        {
            Serializer = JsonUtilities.CreateSerializer();
            Directory = directory;
            ProjectConfiguration = projectConfiguration;
            Root = new JsonFolder(directory, null, this);
            Datafiles = new JsonTopDocumentFindingVisitor()
                .GetRootDocuments(Root)
                .Select(x => (IDatafileInfo)new JsonDatafileInfo(x))
                .ToImmutableArray();
        }

        public JsonFolder Root { get; }

        internal JsonSerializer Serializer { get; }

        private DirectoryInfo Directory { get; }

        public ProjectConfiguration ProjectConfiguration { get; }

        public string RootPath => Directory.FullName;

        public ImmutableArray<IDatafileInfo> Datafiles { get; }

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

        public static JsonWorkspace CreateFromConfigurationInfo(ProjectConfigurationInfo info)
        {
            return new JsonWorkspace(new FileInfo(info.Filepath).Directory, info.Configuration);
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

        private class JsonTopDocumentFindingVisitor
        {
            private Queue<JsonFolder> FoldersToVisit { get; } = new Queue<JsonFolder>();

            public IEnumerable<JsonDocument> GetRootDocuments(JsonFolder initialFolder)
            {
                FoldersToVisit.Enqueue(initialFolder);
                return GetCore().Values();

                IEnumerable<Option<JsonDocument>> GetCore()
                {
                    while (FoldersToVisit.Count > 0)
                    {
                        var folder = FoldersToVisit.Dequeue();
                        var doc = VisitFolder(folder);
                        yield return doc;
                    }
                }
            }

            private Option<JsonDocument> VisitFolder(JsonFolder folder)
            {
                if (folder.GetDocuments().SingleOrDefault() is JsonDocument doc)
                {
                    return doc.Some();
                }
                folder.GetFolders().ForEach(FoldersToVisit.Enqueue);
                return default;
            }
        }
    }
}
