using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using MoreLinq;
using WarHub.ArmouryModel.ProjectSystem;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    /// <summary>
    /// Provides methods to map folder contents to BattleScribe XML documents and load them on demand.
    /// </summary>
    public class XmlWorkspace
    {
        public XmlWorkspace(IEnumerable<IDatafileInfo> files)
        {
            Documents =
                files
                .Select(file => new XmlDocument(file.Filepath.GetXmlDocumentKind(), file, this))
                .ToImmutableArray();
            DocumentsByKind =
                Documents
                .GroupBy(doc => doc.Kind)
                .ToImmutableDictionary(
                    group => group.Key,
                    group => group.ToImmutableArray());
        }

        public ImmutableArray<XmlDocument> Documents { get; }

        public ImmutableDictionary<XmlDocumentKind, ImmutableArray<XmlDocument>> DocumentsByKind { get; }

        /// <summary>
        /// Creates workspace from directory by indexing it's contents (and all subdirectories
        /// if specified using <paramref name="searchOption"/>) for files with well-known extensions.
        /// </summary>
        /// <param name="path">Directory path to search in.</param>
        /// <param name="searchOption">Specify to search all sub-directories.</param>
        /// <returns>Workspace created from the directory with all files with well-known extensions.</returns>
        public static XmlWorkspace CreateFromDirectory(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var dirInfo = new DirectoryInfo(path);
            var files = dirInfo.EnumerateFiles("*", searchOption);
            var datafiles = files.Select(XmlFileExtensions.GetDatafileInfo);
            return new XmlWorkspace(datafiles);
        }

        public static XmlWorkspace CreateFromRepoDistribution(RepoDistribution repo)
        {
            return new XmlWorkspace((repo.Index as IDatafileInfo).Concat(repo.Datafiles));
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
