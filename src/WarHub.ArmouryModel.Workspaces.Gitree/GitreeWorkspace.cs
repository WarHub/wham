using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Optional;
using Optional.Collections;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Workspaces.Gitree.Serialization;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    public sealed class GitreeWorkspace : IWorkspace
    {
        private GitreeWorkspace(GitreeWorkspaceOptions options)
        {
            Serializer = JsonUtilities.CreateSerializer();
            Options = options;
            // TOD validate configuration, handle not-found paths
            var documentFindingVisitor = new GitreeRootFindingVisitor(this);
            Datafiles =
                options.SourceDirectories
                .SelectMany(documentFindingVisitor.GetRootDocuments)
                .Select(x => (IDatafileInfo)new GitreeDatafileInfo(x))
                .ToImmutableArray();
        }

        internal JsonSerializer Serializer { get; }

        public string RootPath => Options.GetDirectoryInfo().FullName;

        public ImmutableArray<IDatafileInfo> Datafiles { get; }

        public GitreeWorkspaceOptions Options { get; }

        public static GitreeWorkspace CreateFromPath(string path)
        {
            if (File.Exists(path))
            {
                return CreateFromConfigurationFile(path);
            }
            return CreateFromDirectory(path);
        }

        public static GitreeWorkspace CreateFromConfigurationFile(string path)
            => new GitreeWorkspace(GitreeWorkspaceOptions.Create(path));

        public static GitreeWorkspace Create(GitreeWorkspaceOptions options)
            => new GitreeWorkspace(options);

        public static GitreeWorkspace CreateFromDirectory(string path)
        {
            var configFiles =
                new DirectoryInfo(path)
                .EnumerateFiles("*" + GitreeWorkspaceOptions.FileExtension)
                .ToList();
            return configFiles.Count switch
            {
                0 => new GitreeWorkspace(GitreeWorkspaceOptions.Create(path)),
                1 => new GitreeWorkspace(GitreeWorkspaceOptions.Create(configFiles[0].FullName)),
                _ => throw new InvalidOperationException("There's more than one project file in the directory"),
            };
        }

        private class GitreeRootFindingVisitor
        {
            public GitreeRootFindingVisitor(GitreeWorkspace workspace)
            {
                Workspace = workspace;
            }

            public GitreeWorkspaceOptions Options => Workspace.Options;

            public GitreeWorkspace Workspace { get; }

            private Queue<GitreeStorageFolderNode> FoldersToVisit { get; } = new Queue<GitreeStorageFolderNode>();

            public IEnumerable<GitreeStorageFileNode> GetRootDocuments(GitreeSourceFolder sourceFolder)
            {
                var initialDir = Options.GetDirectoryInfoFor(sourceFolder);
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
                foreach (var subfolder in folder.GetFolders())
                {
                    FoldersToVisit.Enqueue(subfolder);
                }
                return default;
            }
        }
    }
}
