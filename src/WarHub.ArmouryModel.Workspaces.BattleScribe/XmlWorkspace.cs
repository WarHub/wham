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
            var info = new BattleScribeProjectConfigurationProvider().Create(path);
            return new XmlWorkspace(info);
        }

        public static XmlWorkspace CreateFromConfigurationInfo(ProjectConfigurationInfo info)
        {
            return new XmlWorkspace(info);
        }
    }
}
