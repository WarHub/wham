using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            return file.GetXmlDocumentKind() switch
            {
                XmlDocumentKind.Gamesystem => new LazyWeakXmlDatafileInfo(file.FullName, SourceKind.Gamesystem),
                XmlDocumentKind.Catalogue => new LazyWeakXmlDatafileInfo(file.FullName, SourceKind.Catalogue),
                XmlDocumentKind.Roster => new LazyWeakXmlDatafileInfo(file.FullName, SourceKind.Roster),
                XmlDocumentKind.DataIndex => new LazyWeakXmlDatafileInfo(file.FullName, SourceKind.DataIndex),
                _ => new UnknownTypeDatafileInfo(file.FullName),
            };
        }

        /// <summary>
        /// Reads <see cref="XmlDocumentKind.RepoDistribution"/> <c>.bsr</c> zipped file stream into object model.
        /// </summary>
        /// <param name="stream">Stream of the <c>.bsr</c> file.</param>
        public static RepoDistribution ReadRepoDistribution(this Stream stream)
        {
            using var zip = new ZipArchive(stream);
            var entries = zip.Entries
                .Select(LoadEntry)
                .Where(x => x != null)
                .ToImmutableArray();
            var index = entries.OfType<IDatafileInfo<DataIndexNode>>().Single();
            var datafiles = entries.OfType<IDatafileInfo<CatalogueBaseNode>>().ToImmutableArray();
            return new RepoDistribution(index, datafiles);

            static IDatafileInfo LoadEntry(ZipArchiveEntry entry)
            {
                using var entryStream = entry.Open();
                // TODO log invalid data type
                var node = entryStream.LoadSourceAuto(entry.Name);
                return DatafileInfo.Create(entry.Name, node);
            }
        }

        public static async Task<T> GetDataOrThrowAsync<T>(this IDatafileInfo datafile, CancellationToken cancellationToken = default)
            where T : SourceNode
        {
            var node = await datafile.GetDataOrThrowAsync(cancellationToken);
            return node as T ?? throw new InvalidOperationException($"Datafile '{datafile.Filepath}' has no readable data of type '{typeof(T).Name}'.");
        }

        public static async Task<SourceNode> GetDataOrThrowAsync(this IDatafileInfo datafile, CancellationToken cancellationToken = default)
        {
            var node = await datafile.GetDataAsync(cancellationToken);
            return node ?? throw new InvalidOperationException($"Datafile '{datafile.Filepath}' has no readable data.");
        }

        public static async Task WriteToAsync(this RepoDistribution repoDistribution, Stream stream)
        {
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);
            var datafiles = repoDistribution.Datafiles.Prepend(repoDistribution.Index as IDatafileInfo);
            foreach (var datafile in datafiles)
            {
                var entry = zip.CreateEntry(datafile.Filepath);
#pragma warning disable CA1849 // ZipArchiveEntry.Open is appropriate here since Serialize is synchronous
                using var entryStream = entry.Open();
#pragma warning restore CA1849
                var data = await datafile.GetDataOrThrowAsync();
                data.Serialize(entryStream);
            }
        }

        public static async Task WriteXmlFileAsync(this IDatafileInfo datafile, string filepath)
        {
            using var stream = File.Create(filepath);
            var data = await datafile.GetDataOrThrowAsync();
            data.Serialize(stream);
        }

        public static async Task WriteXmlZippedFileAsync(this IDatafileInfo datafile, string filepath)
        {
            using var fileStream = File.Create(filepath);
            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
#pragma warning disable CA1849 // ZipArchiveEntry.Open is appropriate here since Serialize is synchronous
            using var entryStream = archive.CreateEntry(datafile.GetXmlFilename()).Open();
#pragma warning restore CA1849
            var data = await datafile.GetDataOrThrowAsync();
            data.Serialize(entryStream);
        }

        public static Func<Stream, SourceNode?>? GetLoadingMethod(this XmlDocumentKind kind)
        {
            return kind switch
            {
                XmlDocumentKind.Gamesystem => BattleScribeXml.LoadGamesystem,
                XmlDocumentKind.Catalogue => BattleScribeXml.LoadCatalogue,
                XmlDocumentKind.Roster => BattleScribeXml.LoadRoster,
                XmlDocumentKind.DataIndex => BattleScribeXml.LoadDataIndex,
                _ => null,
            };
        }

        public static bool IsXmlZipped(this XmlDocument document) => document.Kind.IsXmlZipped();

        public static bool IsXmlZipped(this XmlDocumentKind kind)
            => ZippedExtensions.Contains(kind.GetXmlFileExtension());

        public static SourceNode? LoadSourceAuto(this Stream stream, string filename, CancellationToken cancellationToken = default)
        {
            var kind = filename.GetXmlDocumentKind();
            var data = kind.IsXmlZipped()
                ? stream.LoadSourceZipped(cancellationToken)
                : stream.LoadSource(cancellationToken);
            return data;
        }

        private static SourceNode? LoadSource(this Stream stream, CancellationToken cancellationToken = default)
        {
            return BattleScribeXml.LoadAuto(stream, MigrationMode.OnFailure, cancellationToken);
        }

        private static SourceNode? LoadSourceZipped(this Stream stream, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
            if (archive.Entries.Count != 1)
            {
                throw new InvalidOperationException(
                    "File is not a correct BattleScribe ZIP archive," +
                    $" contains {archive.Entries.Count} entries, expected 1.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            using var entryStream = archive.Entries[0].Open();
            return BattleScribeXml.LoadAuto(entryStream, MigrationMode.OnFailure, cancellationToken);
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

        private static async Task<List<T>> ToListAsync<T>(this IEnumerable<Task<T>> collection)
        {
            var list = new List<T>();
            foreach (var task in collection)
            {
                var result = await task;
                list.Add(result);
            }
            return list;
        }

        public static async Task<DataIndexNode> CreateDataIndexAsync(this IWorkspace workspace,
            string repoName, string? repoUrl,
            Func<IDatafileInfo, string> indexDataPathProvider)
        {
            var bsVersion = RootElement.DataIndex.Info().CurrentVersion.BattleScribeString;
            var entries = await
                workspace.Datafiles
                .Where(x => x.DataKind.IsDataCatalogueKind())
                .Select(CreateEntryAsync)
                .ToListAsync();
            return
                NodeFactory.DataIndex(
                    bsVersion,
                    repoName, repoUrl, repositoryUrls: default, dataIndexEntries: entries.ToNodeList());
            async Task<DataIndexEntryNode> CreateEntryAsync(IDatafileInfo datafile)
            {
                var root = await datafile.GetDataAsync();
                var node = root as CatalogueBaseNode ?? throw new ArgumentException("Data must be a CatalogueBase type.", nameof(datafile));
                var path = indexDataPathProvider(datafile);
                var entryKind = datafile.DataKind.GetIndexEntryKindOrUnknown();
                return NodeFactory.DataIndexEntry(path, entryKind, node.Id, node.Name, node.BattleScribeVersion, node.Revision);
            }
        }

        public static async Task<RepoDistribution> CreateRepoDistributionAsync(this IWorkspace workspace, string repoName, string? repoUrl)
        {
            var indexNode = await workspace.CreateDataIndexAsync(repoName, repoUrl, GetXmlFilename);
            var indexDatafile = DatafileInfo.Create(DataIndexFileName, indexNode);
            var datafiles = await workspace.Datafiles
                .Where(x => x.DataKind.IsDataCatalogueKind())
                .Select(async x => DatafileInfo.Create(x.GetXmlFilename(), await x.GetDataOrThrowAsync<CatalogueBaseNode>()))
                .ToListAsync();
            var repo = new RepoDistribution(indexDatafile, datafiles.ToImmutableArray());
            return repo;
        }
    }
}
