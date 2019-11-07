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
    public sealed class GitreeWorkspace : IWorkspace
    {
        private GitreeWorkspace(ProjectConfigurationInfo info)
        {
            Serializer = JsonUtilities.CreateSerializer();
            Info = info;
            // TOD validate configuration, handle not-found paths
            var documentFindingVisitor = new GitreeRootFindingVisitor(info, this);
            Datafiles =
                info.Configuration.SourceDirectories
                .SelectMany(documentFindingVisitor.GetRootDocuments)
                .Select(x => (IDatafileInfo)new GitreeDatafileInfo(x))
                .ToImmutableArray();
        }

        internal JsonSerializer Serializer { get; }

        public string RootPath => Info.GetDirectoryInfo().FullName;

        public ImmutableArray<IDatafileInfo> Datafiles { get; }
        public ProjectConfigurationInfo Info { get; }

        public static GitreeWorkspace CreateFromPath(string path)
        {
            if (File.Exists(path))
            {
                return CreateFromConfigurationFile(path);
            }
            return CreateFromDirectory(path);
        }

        public static GitreeWorkspace CreateFromConfigurationFile(string path)
            => new GitreeWorkspace(new GitreeProjectConfigurationProvider().Create(path));

        public static GitreeWorkspace CreateFromConfigurationInfo(ProjectConfigurationInfo info)
            => new GitreeWorkspace(info);

        public static GitreeWorkspace CreateFromDirectory(string path)
        {
            var configFiles =
                new DirectoryInfo(path)
                .EnumerateFiles("*" + ProjectConfiguration.FileExtension)
                .ToList();
            var configProvider = new GitreeProjectConfigurationProvider();
            return configFiles.Count switch
            {
                0 => new GitreeWorkspace(configProvider.Create(path)),
                1 => new GitreeWorkspace(configProvider.Create(configFiles[0].FullName)),
                _ => throw new InvalidOperationException("There's more than one project file in the directory"),
            };
        }

        private class GitreeRootFindingVisitor
        {
            public GitreeRootFindingVisitor(ProjectConfigurationInfo info, GitreeWorkspace workspace)
            {
                Info = info;
                Workspace = workspace;
            }

            public ProjectConfigurationInfo Info { get; }

            public GitreeWorkspace Workspace { get; }

            private Queue<GitreeStorageFolderNode> FoldersToVisit { get; } = new Queue<GitreeStorageFolderNode>();

            public IEnumerable<GitreeStorageFileNode> GetRootDocuments(SourceFolder sourceFolder)
            {
                var initialDir = Info.GetDirectoryInfoFor(sourceFolder);
                var initialFolder = new GitreeStorageFolderNode(initialDir, null, Workspace);

                FoldersToVisit.Enqueue(initialFolder);
                return GetCore().Values();

                IEnumerable<Option<GitreeStorageFileNode>> GetCore()
                {
                    while (FoldersToVisit.Count > 0)
                    {
                        var folder = FoldersToVisit.Dequeue();
                        yield return VisitFolder(folder);
                    }
                }
            }

            private Option<GitreeStorageFileNode> VisitFolder(GitreeStorageFolderNode folder)
            {
                if (folder.GetDocuments().SingleOrDefault() is GitreeStorageFileNode doc)
                {
                    return doc.Some();
                }
                folder.GetFolders().ForEach(FoldersToVisit.Enqueue);
                return default;
            }
        }
    }
}
