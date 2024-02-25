using System.Collections.Generic;
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
            var documents = GetDocuments(options).ToImmutableArray();
            return new XmlWorkspace(options, documents);

            static IEnumerable<XmlDocument> GetDocuments(XmlWorkspaceOptions options)
            {
                foreach (var filepath in Directory.EnumerateFiles(options.SourceDirectory))
                {
                    var xmlkind = XmlFileExtensions.GetXmlDocumentKind(filepath);
                    IDatafileInfo? datafileInfo;
                    if (xmlkind is XmlDocumentKind.Unknown)
                    {
                        if (!options.IncludeUnknown)
                        {
                            continue;
                        }
                        datafileInfo = new UnknownTypeDatafileInfo(filepath);
                    }
                    else
                    {
                        datafileInfo = new LazyWeakXmlDatafileInfo(filepath, xmlkind.GetSourceKindOrUnknown());
                    }
                    var xmlDocument = new XmlDocument(datafileInfo, xmlkind);
                    yield return xmlDocument;
                }
            }
        }
    }
}
