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
        public XmlWorkspace(ImmutableDictionary<XmlDocumentKind, ImmutableArray<XmlDocument>> files)
        {
            DocumentsByKind = files;
            Documents = files.Values.SelectMany(x => x).ToImmutableArray();
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
            var files = dirInfo.EnumerateFiles("*", searchOption)
                .Select(file => XmlFileExtensions.KindsByExtensions.TryGetValue(file.Extension, out var kind) ? (file, kind) : (null, XmlDocumentKind.Unknown))
                .Where(x => x.kind != XmlDocumentKind.Unknown)
                .GroupBy(x => x.kind, x => x.file)
                .ToImmutableDictionary(x => x.Key, group => ImmutableArray.CreateRange(group.Select(file => new XmlDocument(group.Key, file))));
            return new XmlWorkspace(files);
        }
    }
}
