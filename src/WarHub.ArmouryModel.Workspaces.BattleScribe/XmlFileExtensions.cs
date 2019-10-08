using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Source.BattleScribe;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    /// <summary>
    /// Defines constants for BattleScribe XML file extensions,
    /// maps them to and from <see cref="XmlDocumentKind"/>
    /// and provides sets of zipped and not-zipped extensions.
    /// </summary>
    public static class XmlFileExtensions
    {
        public const string Gamesystem = ".gst";
        public const string GamesystemZipped = ".gstz";
        public const string Catalogue = ".cat";
        public const string CatalogueZipped = ".catz";
        public const string Roster = ".ros";
        public const string RosterZipped = ".rosz";
        public const string DataIndex = ".xml";
        public const string DataIndexZipped = ".bsi";
        public const string RepoDistribution = ".bsr";
        public const string DataIndexFileName = "index.xml";
        public const string DataIndexZippedFileName = "index.bsi";

        static XmlFileExtensions()
        {
            UnzippedExtensions =
                ImmutableHashSet.Create(
                    StringComparer.OrdinalIgnoreCase,
                    Gamesystem,
                    Catalogue,
                    Roster,
                    DataIndex);
            ZippedExtensions =
                ImmutableHashSet.Create(
                    StringComparer.OrdinalIgnoreCase,
                    GamesystemZipped,
                    CatalogueZipped,
                    RosterZipped,
                    DataIndexZipped,
                    RepoDistribution);
            Extensions =
                UnzippedExtensions
                .Concat(ZippedExtensions)
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
            ExtensionsByKinds =
                new Dictionary<XmlDocumentKind, ImmutableArray<string>>
                {
                    [XmlDocumentKind.Gamesystem] = ImmutableArray.Create(Gamesystem, GamesystemZipped),
                    [XmlDocumentKind.Catalogue] = ImmutableArray.Create(Catalogue, CatalogueZipped),
                    [XmlDocumentKind.Roster] = ImmutableArray.Create(Roster, RosterZipped),
                    [XmlDocumentKind.DataIndex] = ImmutableArray.Create(DataIndex, DataIndexZipped),
                    [XmlDocumentKind.RepoDistribution] = ImmutableArray.Create(RepoDistribution),
                }
                .ToImmutableDictionary();
            DocumentKindByExtensions =
                ExtensionsByKinds
                .SelectMany(x => x.Value.Select(value => (x.Key, value)))
                .ToImmutableDictionary(t => t.value, t => t.Key, StringComparer.OrdinalIgnoreCase);
            DocumentKinds =
                new Dictionary<SourceKind, XmlDocumentKind>
                {
                    [SourceKind.Gamesystem] = XmlDocumentKind.Gamesystem,
                    [SourceKind.Catalogue] = XmlDocumentKind.Catalogue,
                    [SourceKind.DataIndex] = XmlDocumentKind.DataIndex,
                    [SourceKind.Roster] = XmlDocumentKind.Roster
                }
                .ToImmutableDictionary();
            SourceKinds = DocumentKinds.ToImmutableDictionary(x => x.Value, x => x.Key);
        }

        public static ImmutableHashSet<string> Extensions { get; }

        public static ImmutableHashSet<string> UnzippedExtensions { get; }

        public static ImmutableHashSet<string> ZippedExtensions { get; }

        public static ImmutableDictionary<SourceKind, XmlDocumentKind> DocumentKinds { get; }

        public static ImmutableDictionary<XmlDocumentKind, SourceKind> SourceKinds { get; }

        public static ImmutableDictionary<XmlDocumentKind, ImmutableArray<string>> ExtensionsByKinds { get; }

        public static ImmutableDictionary<string, XmlDocumentKind> DocumentKindByExtensions { get; }

        public static XmlDocumentKind GetXmlDocumentKind(this FileInfo file)
            => GetKindOrUnknown(file.Extension);

        public static XmlDocumentKind GetXmlDocumentKind(this string path)
            => GetKindOrUnknown(Path.GetExtension(path));

        public static XmlDocumentKind GetXmlDocumentKindOrUnknown(this SourceNode node)
            => node.Kind.GetXmlDocumentKindOrUnknown();

        public static XmlDocumentKind GetXmlDocumentKindOrUnknown(this SourceKind sourceKind)
            => DocumentKinds.TryGetValue(sourceKind, out var kind) ? kind : XmlDocumentKind.Unknown;

        public static SourceKind GetSourceKindOrUnknown(this XmlDocumentKind docKind)
            => SourceKinds.TryGetValue(docKind, out var kind) ? kind : SourceKind.Unknown;

        public static string GetXmlFileExtension(this XmlDocumentKind kind)
            => ExtensionsByKinds[kind][0];

        public static string GetXmlZippedFileExtension(this XmlDocumentKind kind)
            => ExtensionsByKinds[kind].First(ZippedExtensions.Contains);

        public static string GetXmlFilename(this IDatafileInfo datafile)
            => datafile.GetStorageName() + datafile.DataKind.GetXmlDocumentKindOrUnknown().GetXmlFileExtension();

        public static string GetXmlZippedFilename(this IDatafileInfo datafile)
            => datafile.GetStorageName() + datafile.DataKind.GetXmlDocumentKindOrUnknown().GetXmlZippedFileExtension();

        private static XmlDocumentKind GetKindOrUnknown(string extension)
            => DocumentKindByExtensions.TryGetValue(extension, out var kind) ? kind : XmlDocumentKind.Unknown;

        public static IDatafileInfo GetDatafileInfo(this FileInfo file)
        {
            switch (file.GetXmlDocumentKind())
            {
                case XmlDocumentKind.Gamesystem:
                    return new LazyWeakXmlDatafileInfo<GamesystemNode>(file.FullName, SourceKind.Gamesystem);
                case XmlDocumentKind.Catalogue:
                    return new LazyWeakXmlDatafileInfo<CatalogueNode>(file.FullName, SourceKind.Catalogue);
                case XmlDocumentKind.Roster:
                    return new LazyWeakXmlDatafileInfo<RosterNode>(file.FullName, SourceKind.Roster);
                case XmlDocumentKind.DataIndex:
                    return new LazyWeakXmlDatafileInfo<DataIndexNode>(file.FullName, SourceKind.DataIndex);
                default:
                    return new UnknownTypeDatafileInfo(file.FullName);
            }
        }

        /// <summary>
        /// Reads <see cref="XmlDocumentKind.RepoDistribution"/> <c>.bsr</c> zipped file stream into object model.
        /// </summary>
        /// <param name="stream">Stream of the <c>.bsr</c> file.</param>
        /// <returns></returns>
        public static RepoDistribution ReadRepoDistribution(this Stream stream)
        {
            using (var zip = new ZipArchive(stream))
            {
                var entries = zip.Entries
                    .Select(LoadEntry)
                    .Where(x => x != null)
                    .ToImmutableArray();
                var index = entries.OfType<IDatafileInfo<DataIndexNode>>().Single();
                var datafiles = entries.OfType<IDatafileInfo<CatalogueBaseNode>>().ToImmutableArray();
                return new RepoDistribution(index, datafiles);
            }

            IDatafileInfo<SourceNode> LoadEntry(ZipArchiveEntry entry)
            {
                using (var entryStream = entry.Open())
                {
                    // TODO log invalid data type
                    return entryStream.LoadSourceAuto(entry.Name);
                }
            }
        }

        public static void WriteTo(this RepoDistribution repoDistribution, Stream stream)
        {
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var datafiles = repoDistribution.Datafiles.Prepend(repoDistribution.Index as IDatafileInfo);
                foreach (var datafile in datafiles)
                {
                    var entry = zip.CreateEntry(datafile.Filepath);
                    using (var entryStream = entry.Open())
                    {
                        datafile.GetData().Serialize(entryStream);
                    }
                }
            }
        }

        public static void WriteXmlFile(this IDatafileInfo datafile, string filepath)
        {
            using (var stream = File.Create(filepath))
            {
                datafile.GetData().Serialize(stream);
            }
        }

        public static void WriteXmlZippedFile(this IDatafileInfo datafile, string filepath)
        {
            using (var fileStream = File.Create(filepath))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            using (var entryStream = archive.CreateEntry(datafile.GetXmlFilename()).Open())
            {
                datafile.GetData().Serialize(entryStream);
            }
        }

        public static Func<Stream, SourceNode> GetLoadingMethod(this XmlDocumentKind kind)
        {
            switch (kind)
            {
                case XmlDocumentKind.Gamesystem: return BattleScribeXml.LoadGamesystem;
                case XmlDocumentKind.Catalogue: return BattleScribeXml.LoadCatalogue;
                case XmlDocumentKind.Roster: return BattleScribeXml.LoadRoster;
                case XmlDocumentKind.DataIndex: return BattleScribeXml.LoadDataIndex;
                case XmlDocumentKind.Unknown:
                case XmlDocumentKind.RepoDistribution:
                default:
                    return null;
            }
        }

        public static IDatafileInfo<SourceNode> LoadSourceAuto(this Stream stream, string filename)
        {
            var kind = filename.GetXmlDocumentKind();
            var data = ZippedExtensions.Contains(Path.GetExtension(filename))
                ? stream.LoadSourceZipped(kind)
                : stream.LoadSource(kind);
            return DatafileInfo.Create(filename, data);
        }

        public static IDatafileInfo<SourceNode> LoadSourceFileAuto(this string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return stream.LoadSourceAuto(path);
            }
        }

        public static SourceNode LoadSource(this Stream stream, XmlDocumentKind kind)
        {
            return kind.GetLoadingMethod()?.Invoke(stream);
        }

        public static SourceNode LoadSourceZipped(this Stream stream, XmlDocumentKind kind)
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
            {
                if (archive.Entries.Count != 1)
                {
                    throw new InvalidOperationException(
                        $"File is not a correct BattleScribe ZIP archive," +
                        $" contains {archive.Entries.Count} entries, expected 1.");
                }
                using (var entryStream = archive.Entries[0].Open())
                {
                    return entryStream.LoadSource(kind);
                }
            }
        }

        public static IEnumerable<XmlDocument> GetDocuments(this XmlWorkspace workspace, params SourceKind[] kinds)
            => workspace.GetDocuments(kinds.AsEnumerable());

        public static IEnumerable<XmlDocument> GetDocuments(this XmlWorkspace workspace, IEnumerable<SourceKind> kinds)
        {
            var kindSet = kinds
                .Select(x => x.GetXmlDocumentKindOrUnknown())
                .ToImmutableHashSet();
            return workspace.Documents.Where(x => kindSet.Contains(x.Kind));
        }

        public static DataIndexNode CreateDataIndex(this IWorkspace workspace,
            string repoName, string repoUrl,
            Func<IDatafileInfo, string> indexDataPathProvider)
        {
            var entries =
                workspace.Datafiles
                .Where(x => x.DataKind.IsDataCatalogueKind())
                .Select(CreateEntry)
                .ToNodeList();
            return
                NodeFactory.DataIndex(
                    RootElement.DataIndex.Info().CurrentVersion.BattleScribeString,
                    repoName, repoUrl, repositoryUrls: default, dataIndexEntries: entries);
            DataIndexEntryNode CreateEntry(IDatafileInfo datafile)
            {
                var node = (CatalogueBaseNode)datafile.GetData();
                var path = indexDataPathProvider(datafile);
                var entryKind = datafile.DataKind.GetIndexEntryKindOrUnknown();
                return NodeFactory.DataIndexEntry(path, entryKind, node.Id, node.Name, node.BattleScribeVersion, node.Revision);
            }
        }

        public static RepoDistribution CreateRepoDistribution(this IWorkspace workspace, string repoName, string repoUrl)
        {
            var indexNode = workspace.CreateDataIndex(repoName, repoUrl, x => x.GetXmlFilename());
            var indexDatafile = DatafileInfo.Create(DataIndexFileName, indexNode);
            var datafiles = workspace.Datafiles
                .Where(x => x.DataKind.IsDataCatalogueKind())
                .Select(x => DatafileInfo.Create(x.GetXmlFilename(), x.GetData()))
                .OfType<IDatafileInfo<CatalogueBaseNode>>()
                .ToImmutableArray();
            var repo = new RepoDistribution(indexDatafile, datafiles);
            return repo;
        }
    }
}
