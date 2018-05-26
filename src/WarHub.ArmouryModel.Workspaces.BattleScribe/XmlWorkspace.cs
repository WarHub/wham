using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using MoreLinq;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    /// <summary>
    /// Provides methods to map folder contents to BattleScribe XML documents and load them on demand.
    /// </summary>
    public class XmlWorkspace : IWorkspace
    {
        private XmlWorkspace(ProjectConfigurationInfo info)
        {
            var files = info.Configuration.SourceDirectories
                .SelectMany(x => info.GetDirectoryInfoFor(x).EnumerateFiles());
            Datafiles = files.Select(XmlFileExtensions.GetDatafileInfo).ToImmutableArray();
            Documents =
                Datafiles
                .Select(file => new XmlDocument(file.Filepath.GetXmlDocumentKind(), file, this))
                .ToImmutableArray();
            DocumentsByKind =
                Documents
                .GroupBy(doc => doc.Kind)
                .ToImmutableDictionary(
                    group => group.Key,
                    group => group.ToImmutableArray());
            Info = info;
        }

        public ImmutableArray<XmlDocument> Documents { get; }

        public ImmutableDictionary<XmlDocumentKind, ImmutableArray<XmlDocument>> DocumentsByKind { get; }

        public string RootPath { get; }

        public ImmutableArray<IDatafileInfo> Datafiles { get; }

        public ProjectConfigurationInfo Info { get; }

        /// <summary>
        /// Creates workspace from directory by indexing it's contents for files with well-known extensions.
        /// </summary>
        /// <param name="path">Directory path to search in.</param>
        /// <returns>Workspace created from the directory with all files with well-known extensions.</returns>
        public static XmlWorkspace CreateFromDirectory(string path)
        {
            var dirInfo = new DirectoryInfo(path);

            var info = new BattleScribeProjectConfigurationProvider().Create(path);
            return new XmlWorkspace(info);
        }

        public static XmlWorkspace CreateFromConfigurationInfo(ProjectConfigurationInfo info)
        {
            return new XmlWorkspace(info);
        }

        private static string GetLongestCommonPath(IEnumerable<FileInfo> files)
        {
            return files
                .Select(x => x.Directory)
                .Select(
                    x => MoreEnumerable
                    .Generate(x, y => y.Parent)
                    .Select(y => y.FullName)
                    .Reverse()
                    .ToImmutableArray()
                    .AsEnumerable())
                .Aggregate(
                    (left, right) =>
                    left.Zip(right, (x1, x2) => x1 == x2 ? x1 : null)
                    .Where(x => x != null))
                .LastOrDefault();
        }

        public DataIndexNode CreateDataIndex(string repoName, string repoUrl)
        {
            var entries =
                Documents
                .Where(x => XmlFileExtensions.DataCatalogueKinds.Contains(x.Kind))
                .Select(CreateEntry)
                .ToNodeList();
            return NodeFactory.DataIndex(ProjectToolset.Version, repoName, repoUrl, dataIndexEntries: entries);
            DataIndexEntryNode CreateEntry(XmlDocument doc)
            {
                var node = (CatalogueBaseNode)doc.GetRoot();
                var path = Path.GetFileName(doc.Filepath);
                var entryKind = doc.Kind == XmlDocumentKind.Gamesystem
                    ? DataIndexEntryKind.Gamesystem : DataIndexEntryKind.Catalogue;
                return NodeFactory.DataIndexEntry(path, entryKind, node.Id, node.Name, node.BattleScribeVersion, node.Revision);
            }
        }

        public RepoDistribution CreateRepoDistribution(string repoName, string repoUrl)
        {
            var indexNode = CreateDataIndex(repoName, repoUrl);
            var indexDatafile = DatafileInfo.Create(XmlFileExtensions.DataIndexFileFullName, indexNode);
            var datafiles = Documents
                .Where(x => XmlFileExtensions.DataCatalogueKinds.Contains(x.Kind))
                .Select(CreateDatafileInfo)
                .ToImmutableArray();
            var repo = new RepoDistribution(indexDatafile, datafiles);
            return repo;

            IDatafileInfo<CatalogueBaseNode> CreateDatafileInfo(XmlDocument doc)
            {
                var node = (CatalogueBaseNode)doc.GetRoot();
                return DatafileInfo.Create(doc.Filepath, node);
            }
        }
    }
}
