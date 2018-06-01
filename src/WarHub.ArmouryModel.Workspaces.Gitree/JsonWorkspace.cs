using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using MoreLinq;
using Newtonsoft.Json;
using Optional;
using Optional.Collections;
using WarHub.ArmouryModel.ProjectModel;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{

    public class JsonWorkspace : IWorkspace
    {
        private JsonWorkspace(ProjectConfigurationInfo info)
        {
            Serializer = JsonUtilities.CreateSerializer();
            Info = info;
            // TOD validate configuration, handle not-found paths
            var documentFindingVisitor = new JsonTopDocumentFindingVisitor(info, this);
            Datafiles = 
                info.Configuration.SourceDirectories
                .SelectMany(documentFindingVisitor.GetRootDocuments)
                .Select(x => (IDatafileInfo)new JsonDatafileInfo(x))
                .ToImmutableArray();
        }

        public JsonFolder Root { get; }

        internal JsonSerializer Serializer { get; }

        private ProjectConfiguration ProjectConfiguration => Info.Configuration;

        public string RootPath => Info.GetDirectoryInfo().FullName;

        public ImmutableArray<IDatafileInfo> Datafiles { get; }
        public ProjectConfigurationInfo Info { get; }

        public static JsonWorkspace CreateFromPath(string path)
        {
            if (File.Exists(path))
            {
                return CreateFromConfigurationFile(path);
            }
            return CreateFromDirectory(path);
        }

        public static JsonWorkspace CreateFromConfigurationFile(string path)
            => new JsonWorkspace(new JsonFolderProjectConfigurationProvider().Create(path));

        public static JsonWorkspace CreateFromConfigurationInfo(ProjectConfigurationInfo info)
            => new JsonWorkspace(info);

        public static JsonWorkspace CreateFromDirectory(string path)
        {
            var configFiles =
                new DirectoryInfo(path)
                .EnumerateFiles("*" + ProjectConfiguration.FileExtension)
                .ToList();
            var configProvider = new JsonFolderProjectConfigurationProvider();
            switch (configFiles.Count)
            {
                case 0:
                    return new JsonWorkspace(configProvider.Create(path));
                case 1:
                    return new JsonWorkspace(configProvider.Create(configFiles[0].FullName));
                default:
                    throw new InvalidOperationException("There's more than one project file in the directory");
            }
        }

        private class JsonTopDocumentFindingVisitor
        {
            public JsonTopDocumentFindingVisitor(ProjectConfigurationInfo info, JsonWorkspace workspace)
            {
                Info = info;
                Workspace = workspace;
            }

            public ProjectConfigurationInfo Info { get; }

            public JsonWorkspace Workspace { get; }

            private Queue<JsonFolder> FoldersToVisit { get; } = new Queue<JsonFolder>();

            public IEnumerable<JsonDocument> GetRootDocuments(SourceFolder sourceFolder)
            {
                var initialDir = Info.GetDirectoryInfoFor(sourceFolder);
                var initialFolder = new JsonFolder(initialDir, null, Workspace);

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
