using System.Collections.Immutable;
using System.Linq;
using WarHub.ArmouryModel.ProjectModel;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    /// <summary>
    /// Provides methods to map folder contents to BattleScribe XML documents and load them on demand.
    /// </summary>
    public sealed class XmlWorkspace : IWorkspace
    {
        private XmlWorkspace(ProjectConfigurationInfo info, ImmutableArray<XmlDocument> documents)
        {
            Info = info;
            Documents = documents.Select(x => x.WithWorkspace(this)).ToImmutableArray();
        }

        private string rootPath;
        private ImmutableArray<IDatafileInfo>? datafiles;
        private ImmutableDictionary<XmlDocumentKind, ImmutableArray<XmlDocument>> documentsByKind;

        public ProjectConfigurationInfo Info { get; }

        public ImmutableArray<XmlDocument> Documents { get; }

        public ImmutableDictionary<XmlDocumentKind, ImmutableArray<XmlDocument>> DocumentsByKind =>
            documentsByKind ??= Documents.GroupBy(doc => doc.Kind).ToImmutableDictionary(
                    group => group.Key,
                    group => group.ToImmutableArray());

        public string RootPath => rootPath ??= Info.GetDirectoryInfo().FullName;

        public ImmutableArray<IDatafileInfo> Datafiles => datafiles ??= Documents.Select(x => x.DatafileInfo).ToImmutableArray();

        /// <summary>
        /// Creates workspace from directory by indexing it's contents for files with well-known extensions.
        /// </summary>
        /// <param name="path">Directory path to search in.</param>
        /// <returns>Workspace created from the directory with all files with well-known extensions.</returns>
        public static XmlWorkspace CreateFromDirectory(string path)
        {
            var info = new BattleScribeProjectConfigurationProvider().Create(path);
            return CreateFromConfigurationInfo(info);
        }

        public static XmlWorkspace CreateFromDocuments(ImmutableArray<XmlDocument> documents)
        {
            return new XmlWorkspace(new BattleScribeProjectConfigurationProvider().Empty, documents);
        }

        public static XmlWorkspace CreateFromConfigurationInfo(ProjectConfigurationInfo info)
        {
            var files = info.Configuration.SourceDirectories
                .SelectMany(x => info.GetDirectoryInfoFor(x).EnumerateFiles());
            var datafiles = files.Select(XmlFileExtensions.GetDatafileInfo).ToImmutableArray();
            var documents = datafiles.Select(file => new XmlDocument(file.Filepath.GetXmlDocumentKind(), file, null)).ToImmutableArray();
            return new XmlWorkspace(info, documents);
        }
    }
}
