using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public static class ProjectConfigurationExtensions
    {
        public const string DataIndexFileName = "index.xml";
        public const string DataIndexZippedFileName = "index.bsi";

        static ProjectConfigurationExtensions()
        {
            DataCatalogueKinds =
                ImmutableHashSet.Create(
                    SourceKind.Gamesystem,
                    SourceKind.Catalogue);

            FolderKindsBySourceKinds =
                new Dictionary<SourceKind, ImmutableHashSet<SourceFolderKind>>
                {
                    [SourceKind.Catalogue] = ImmutableHashSet.Create(SourceFolderKind.All, SourceFolderKind.Catalogues),
                    [SourceKind.Gamesystem] = ImmutableHashSet.Create(SourceFolderKind.All, SourceFolderKind.Gamesystems)
                }
                .ToImmutableDictionary();

            DataIndexKinds =
                new Dictionary<SourceKind, DataIndexEntryKind>
                {
                    [SourceKind.Catalogue] = DataIndexEntryKind.Catalogue,
                    [SourceKind.Gamesystem] = DataIndexEntryKind.Gamesystem
                }
                .ToImmutableDictionary();

            SourceKindsByDataIndexKinds = DataIndexKinds.ToImmutableDictionary(x => x.Value, x => x.Key);

            SourceKindsByFolderKinds = FolderKindsBySourceKinds
                .SelectMany(x => x.Value.Select(folderKind => (folderKind, sourceKind: x.Key)))
                .GroupBy(x => x.folderKind, x => x.sourceKind)
                .ToImmutableDictionary(x => x.Key, x => x.ToImmutableHashSet());
        }

        public static ImmutableHashSet<SourceKind> DataCatalogueKinds { get; }
        public static ImmutableDictionary<SourceKind, ImmutableHashSet<SourceFolderKind>> FolderKindsBySourceKinds { get; }
        public static ImmutableDictionary<SourceKind, DataIndexEntryKind> DataIndexKinds { get; }
        public static ImmutableDictionary<DataIndexEntryKind, SourceKind> SourceKindsByDataIndexKinds { get; }
        public static ImmutableDictionary<SourceFolderKind, ImmutableHashSet<SourceKind>> SourceKindsByFolderKinds { get; }

        public static ImmutableHashSet<SourceFolderKind> FolderKinds(this SourceKind sourceKind)
            => FolderKindsBySourceKinds[sourceKind];

        public static ImmutableHashSet<SourceKind> SourceKinds(this SourceFolderKind folderKind)
            => SourceKindsByFolderKinds[folderKind];

        public static DataIndexEntryKind GetIndexEntryKindOrUnknown(this SourceKind sourceKind)
            => DataIndexKinds.TryGetValue(sourceKind, out var kind) ? kind : DataIndexEntryKind.Unknown;

        public static IEnumerable<SourceFolder> GetSourceFolders(this ProjectConfiguration config, SourceKind kind)
        {
            var folderKinds = kind.FolderKinds();
            return config.SourceDirectories.Where(x => folderKinds.Contains(x.Kind));
        }

        public static bool IsDataCatalogueKind(this SourceKind kind) => DataCatalogueKinds.Contains(kind);

        public static SourceFolder GetSourceFolder(this ProjectConfiguration config, SourceKind kind)
        {
            return config.GetSourceFolders(kind).First();
        }

        public static void WriteFile(this ProjectConfigurationInfo configInfo)
        {
            using (var stream = File.OpenWrite(configInfo.Filepath))
            {
                configInfo.Configuration.Serialize(stream);
            }
        }

        public static void Serialize(this ProjectConfiguration configuration, Stream stream)
        {
            var serializer = JsonUtilities.CreateSerializer();
            using (var textWriter = new StreamWriter(stream))
            {
                serializer.Serialize(textWriter, configuration);
            }
        }

        public static string GetFullPath(this ProjectConfigurationInfo configInfo, SourceKind kind)
            => configInfo.GetFullPath(configInfo.Configuration.GetSourceFolder(kind));

        public static string GetFullPath(this ProjectConfigurationInfo configInfo, SourceFolder folder)
            => Path.Combine(Path.GetDirectoryName(configInfo.Filepath), folder.Subpath);

        public static DirectoryInfo GetDirectoryInfo(this ProjectConfigurationInfo configInfo)
            => new DirectoryInfo(Path.GetDirectoryName(configInfo.Filepath));

        public static string GetFilename(this ProjectConfigurationInfo configInfo)
            => Path.GetFileName(configInfo.Filepath);

        public static DirectoryInfo GetDirectoryInfoFor(this ProjectConfigurationInfo configInfo, SourceFolder folder)
            => new DirectoryInfo(configInfo.GetFullPath(folder));

        public static DataIndexNode CreateDataIndex(this IWorkspace workspace, string repoName, string repoUrl)
        {
            var entries =
                workspace.Datafiles
                .Where(x => x.DataKind.IsDataCatalogueKind())
                .Select(CreateEntry)
                .ToNodeList();
            // TODO use BattleScribe version for DataIndex bs version
            return NodeFactory.DataIndex(ProjectToolset.BattleScribeDataIndexFormatVersion, repoName, repoUrl, dataIndexEntries: entries);
            DataIndexEntryNode CreateEntry(IDatafileInfo datafile)
            {
                var node = (CatalogueBaseNode)datafile.GetData();
                var path = Path.GetFileName(datafile.Filepath);
                var entryKind = datafile.DataKind.GetIndexEntryKindOrUnknown();
                return NodeFactory.DataIndexEntry(path, entryKind, node.Id, node.Name, node.BattleScribeVersion, node.Revision);
            }
        }

        public static RepoDistribution CreateRepoDistribution(this IWorkspace workspace, string repoName, string repoUrl)
        {
            var indexNode = workspace.CreateDataIndex(repoName, repoUrl);
            var indexDatafile = DatafileInfo.Create(DataIndexFileName, indexNode);
            var datafiles = workspace.Datafiles
                .Where(x => x.DataKind.IsDataCatalogueKind())
                .Select(x => DatafileInfo.Create(Path.GetFileName(x.Filepath), x.GetData()))
                .OfType<IDatafileInfo<CatalogueBaseNode>>()
                .ToImmutableArray();
            var repo = new RepoDistribution(indexDatafile, datafiles);
            return repo;
        }
    }
}
