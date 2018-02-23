using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    /// <summary>
    /// Provides methods to map folder contents to BattleScribe XML documents and load them on demand.
    /// </summary>
    public class XmlWorkspace
    {
        public XmlWorkspace(IEnumerable<FileInfo> files)
        {
            Documents =
                files
                .Select(file => new XmlDocument(file.GetXmlDocumentKind(), file, this))
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
            return new XmlWorkspace(files);
        }
    }
}
