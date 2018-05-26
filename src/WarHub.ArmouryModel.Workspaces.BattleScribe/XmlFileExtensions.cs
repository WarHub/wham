using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MoreLinq;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Source.BattleScribe;

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
        public const string DataIndexFileNameNoExtension = "index";
        public const string DataIndexFileFullName = DataIndexFileNameNoExtension + DataIndex;

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
            DataCatalogueKinds =
                ImmutableHashSet.Create(
                    XmlDocumentKind.Gamesystem,
                    XmlDocumentKind.Catalogue);
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

        /// <summary>
        /// Gets the set containing <see cref="XmlDocumentKind.Gamesystem"/>
        /// and <see cref="XmlDocumentKind.Catalogue"/>.
        /// </summary>
        public static ImmutableHashSet<XmlDocumentKind> DataCatalogueKinds { get; }

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

        public static string GetFileExtension(this XmlDocumentKind kind)
            => ExtensionsByKinds[kind].First();

        public static string GetZippedFileExtension(this XmlDocumentKind kind)
            => ExtensionsByKinds[kind].First(ZippedExtensions.Contains);

        private static XmlDocumentKind GetKindOrUnknown(string extension)
            => DocumentKindByExtensions.TryGetValue(extension, out var kind) ? kind : XmlDocumentKind.Unknown;

        public static IDatafileInfo GetDatafileInfo(this FileInfo file)
        {
            switch (file.GetXmlDocumentKind())
            {
                case XmlDocumentKind.Gamesystem: return new LazyWeakDatafileInfo<GamesystemNode>(file.FullName);
                case XmlDocumentKind.Catalogue: return new LazyWeakDatafileInfo<CatalogueNode>(file.FullName);
                case XmlDocumentKind.Roster: return new LazyWeakDatafileInfo<RosterNode>(file.FullName);
                case XmlDocumentKind.DataIndex: return new LazyWeakDatafileInfo<DataIndexNode>(file.FullName);
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

        public static void WriteRepoDistribution(this Stream stream, RepoDistribution repoDistribution)
        {
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var datafiles = (repoDistribution.Index as IDatafileInfo).Concat(repoDistribution.Datafiles);
                foreach (var datafile in datafiles)
                {
                    var entry = zip.CreateEntry(datafile.Filepath);
                    using (var entryStream = entry.Open())
                    {
                        datafile.Data.Serialize(entryStream);
                    }
                }
            }
        }

        public static Func<Stream, SourceNode> GetLoadingMethod(this XmlDocumentKind kind)
        {
            return kind.Match<Func<Stream, SourceNode>>(
                gamesystem: BattleScribeXml.LoadGamesystem,
                catalogue: BattleScribeXml.LoadCatalogue,
                roster: BattleScribeXml.LoadRoster,
                dataIndex: BattleScribeXml.LoadDataIndex,
                repoDistribution: null,
                unknown: null,
                @default: null);
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

        public static T Match<T>(
            this XmlDocumentKind kind,
            T gamesystem,
            T catalogue,
            T roster,
            T dataIndex,
            T repoDistribution,
            T unknown,
            T @default = default)
        {
            switch (kind)
            {
                case XmlDocumentKind.Gamesystem:
                    return gamesystem;
                case XmlDocumentKind.Catalogue:
                    return catalogue;
                case XmlDocumentKind.Roster:
                    return roster;
                case XmlDocumentKind.DataIndex:
                    return dataIndex;
                case XmlDocumentKind.RepoDistribution:
                    return repoDistribution;
                case XmlDocumentKind.Unknown:
                    return unknown;
                default:
                    return @default;
            }
        }
    }
}
