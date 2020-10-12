using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.Gitree.Serialization;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    public static class GitreeWorkspaceOptionsExtensions
    {
        static GitreeWorkspaceOptionsExtensions()
        {
            FolderKindsBySourceKinds =
                new Dictionary<SourceKind, ImmutableHashSet<GitreeSourceFolderKind>>
                {
                    [SourceKind.Catalogue] = ImmutableHashSet.Create(GitreeSourceFolderKind.All, GitreeSourceFolderKind.Catalogues),
                    [SourceKind.Gamesystem] = ImmutableHashSet.Create(GitreeSourceFolderKind.All, GitreeSourceFolderKind.Gamesystems)
                }
                .ToImmutableDictionary();

            SourceKindsByFolderKinds = FolderKindsBySourceKinds
                .SelectMany(x => x.Value.Select(folderKind => (folderKind, sourceKind: x.Key)))
                .GroupBy(x => x.folderKind, x => x.sourceKind)
                .ToImmutableDictionary(x => x.Key, x => x.ToImmutableHashSet());
        }

        public static ImmutableDictionary<SourceKind, ImmutableHashSet<GitreeSourceFolderKind>> FolderKindsBySourceKinds { get; }
        public static ImmutableDictionary<GitreeSourceFolderKind, ImmutableHashSet<SourceKind>> SourceKindsByFolderKinds { get; }

        public static ImmutableHashSet<GitreeSourceFolderKind> FolderKinds(this SourceKind sourceKind)
            => FolderKindsBySourceKinds[sourceKind];

        public static ImmutableHashSet<SourceKind> SourceKinds(this GitreeSourceFolderKind folderKind)
            => SourceKindsByFolderKinds[folderKind];

        public static IEnumerable<GitreeSourceFolder> GetSourceFolders(this GitreeWorkspaceOptions config, SourceKind kind)
        {
            var folderKinds = kind.FolderKinds();
            return config.SourceDirectories.Where(x => folderKinds.Contains(x.Kind));
        }

        public static GitreeSourceFolder GetSourceFolder(this GitreeWorkspaceOptions config, SourceKind kind)
        {
            return config.GetSourceFolders(kind).First();
        }

        public static void WriteFile(this GitreeWorkspaceOptions options)
        {
            using var stream = File.OpenWrite(options.Filepath);
            options.Serialize(stream);
        }

        public static void Serialize(this GitreeWorkspaceOptions options, Stream stream)
        {
            var serializer = JsonUtilities.CreateSerializer();
            using var textWriter = new StreamWriter(stream);
            serializer.Serialize(textWriter, options);
        }

        public static string GetFullPath(this GitreeWorkspaceOptions configInfo, SourceKind kind)
            => configInfo.GetFullPath(configInfo.GetSourceFolder(kind));

        public static string GetFullPath(this GitreeWorkspaceOptions configInfo, GitreeSourceFolder folder)
            => Path.Combine(Path.GetDirectoryName(configInfo.Filepath) ?? "", folder.Subpath);

        public static DirectoryInfo GetDirectoryInfo(this GitreeWorkspaceOptions configInfo)
            => new DirectoryInfo(Path.GetDirectoryName(configInfo.Filepath) ?? "");

        public static string GetFilename(this GitreeWorkspaceOptions configInfo)
            => Path.GetFileName(configInfo.Filepath);

        public static DirectoryInfo GetDirectoryInfoFor(this GitreeWorkspaceOptions configInfo, GitreeSourceFolder folder)
            => new DirectoryInfo(configInfo.GetFullPath(folder));
    }
}
