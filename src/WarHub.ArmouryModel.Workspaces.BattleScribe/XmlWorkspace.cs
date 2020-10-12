using System.Collections.Immutable;
using System.IO;
using System.Linq;
using WarHub.ArmouryModel.ProjectModel;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    /// <summary>
    /// Provides methods to map folder contents to BattleScribe XML documents and load them on demand.
    /// </summary>
    public sealed class XmlWorkspace : IWorkspace
    {
        private XmlWorkspace(XmlWorkspaceOptions info, ImmutableArray<XmlDocument> documents)
        {
            Options = info;
            Documents = documents.Select(x => x with { Workspace = this }).ToImmutableArray();
        }

        private string? rootPath;
        private ImmutableArray<IDatafileInfo>? datafiles;
        private ImmutableDictionary<XmlDocumentKind, ImmutableArray<XmlDocument>>? documentsByKind;

        public XmlWorkspaceOptions Options { get; }

        public ImmutableArray<XmlDocument> Documents { get; }

        public ImmutableDictionary<XmlDocumentKind, ImmutableArray<XmlDocument>> DocumentsByKind =>
            documentsByKind ??= Documents.GroupBy(doc => doc.Kind).ToImmutableDictionary(
                    group => group.Key,
                    group => group.ToImmutableArray());

        public string RootPath => rootPath ??= new DirectoryInfo(Options.SourceDirectory).FullName;

        public ImmutableArray<IDatafileInfo> Datafiles => datafiles ??= Documents.Select(x => x.DatafileInfo).ToImmutableArray();

        /// <summary>
        /// Creates workspace from directory by indexing it's contents for files with well-known extensions.
        /// </summary>
        /// <param name="path">Directory path to search in.</param>
        /// <returns>Workspace created from the directory with all files with well-known extensions.</returns>
        public static XmlWorkspace CreateFromDirectory(string path)
        {
            return Create(new XmlWorkspaceOptions
            {
                SourceDirectory = path
            });
        }

        public static XmlWorkspace CreateFromDocuments(ImmutableArray<XmlDocument> documents)
        {
            return new XmlWorkspace(new XmlWorkspaceOptions(), documents);
        }

        public static XmlWorkspace Create(XmlWorkspaceOptions options)
        {
            var files = new DirectoryInfo(options.SourceDirectory).EnumerateFiles();
            var datafiles = files.Select(XmlFileExtensions.GetDatafileInfo).ToImmutableArray();
            var documents = datafiles.Select(file => new XmlDocument(file, file.Filepath.GetXmlDocumentKind())).ToImmutableArray();
            return new XmlWorkspace(options, documents);
        }
    }
}
