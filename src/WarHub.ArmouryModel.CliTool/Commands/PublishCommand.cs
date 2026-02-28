using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Source.XmlFormat;
using WarHub.ArmouryModel.Workspaces.BattleScribe;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public partial class PublishCommand : CommandBase
    {
        internal static readonly string[] ArtifactNames = new[] { "xml", "zip", "index", "bsi", "bsr" };

        private sealed record Options(DirectoryInfo Source, DirectoryInfo Output, string RepoName, string Filename)
        {
            public ImmutableArray<ArtifactType> Artifacts { get; init; } = ImmutableArray<ArtifactType>.Empty;
            public DirectoryInfo Source { get; init; } = Source;
            public DirectoryInfo Output { get; init; } = Output;
            public Uri? Url { get; init; }
            public ImmutableArray<Uri> AdditionalUrls { get; init; } = ImmutableArray<Uri>.Empty;
            public bool UrlOnlyIndex { get; init; }
            public string RepoName { get; init; } = RepoName;
            public string Filename { get; init; } = Filename;
        }

        public enum ArtifactType
        {
            None,
            XmlDatafiles,
            ZippedXmlDatafiles,
            Index,
            ZippedIndex,
            RepoDistribution
        }

        public async Task RunAsync(
            IEnumerable<string> artifacts,
            DirectoryInfo source,
            DirectoryInfo output,
            Uri? url,
            IEnumerable<Uri> additionalUrls,
            bool urlOnlyIndex,
            string? repoName,
            string? filename,
            string? verbosity)
        {
            SetupLogger(verbosity);

            var artifactTypes = artifacts
                .Distinct()
                .Select(ParseArtifactType)
                .Where(x => x != ArtifactType.None)
                .ToImmutableArray();
            if (artifactTypes.Length == 0)
            {
                Log.Information("Nothing to do.");
                return;
            }
            output ??= new DirectoryInfo("artifacts");
            Log.Debug("Writing artifacts to: {Destination}", output);
            output.Create();

            Log.Debug("Loading workspace...");
            var workspace = XmlWorkspace.CreateFromDirectory(source.FullName);
            Log.Debug(
                "Workspace loaded. {DatafileCount} datafiles discovered.",
                workspace.Datafiles.Count(x => x.DataKind.IsDataCatalogueKind()));

            await CheckBattleScribeVersionCompatibilityAsync(workspace);

            var resolvedRepoName = string.IsNullOrWhiteSpace(repoName) ? await GetRepoNameFallbackAsync(workspace) : repoName;
            Log.Debug("Repository name used is: {RepoName}", resolvedRepoName);

            var resolvedFilename = string.IsNullOrWhiteSpace(filename) ? source.Name : filename;

            var options = new Options(source, output, resolvedRepoName, resolvedFilename)
            {
                Artifacts = artifactTypes,
                Url = url,
                AdditionalUrls = additionalUrls?.ToImmutableArray() ?? ImmutableArray<Uri>.Empty,
                UrlOnlyIndex = urlOnlyIndex,
            };

            foreach (var artifactType in options.Artifacts)
            {
                await CreateArtifactAsync(workspace, options, artifactType);
            }
            Log.Verbose("All done.");
        }

        private async Task CheckBattleScribeVersionCompatibilityAsync(XmlWorkspace workspace)
        {
            foreach (var file in workspace.Datafiles)
            {
                var node = await file.GetDataAsync();
                if (node is IRootNode rootNode && !string.IsNullOrWhiteSpace(rootNode.BattleScribeVersion))
                {
                    var version = BattleScribeVersion.Parse(rootNode.BattleScribeVersion);
                    var maxSupportedVersion = node.Kind.ToRootElement().Info().CurrentVersion;
                    if (version > maxSupportedVersion)
                    {
                        Log.Warning(
                            "Processing {File} which has BattleScribeVersion higher than supported" +
                            " ({NodeVersion} > {SupportedVersion}).",
                            file, version, maxSupportedVersion);
                    }
                }
            }
        }

        private async Task CreateArtifactAsync(IWorkspace workspace, Options options, ArtifactType artifactType)
        {
            Log.Information("Creating artifact: {Artifact}", artifactType);
            switch (artifactType)
            {
                case ArtifactType.XmlDatafiles:
                    await PublishXmlAsync(workspace, options);
                    break;
                case ArtifactType.ZippedXmlDatafiles:
                    await PublishXmlZippedAsync(workspace, options);
                    break;
                case ArtifactType.Index:
                    await PublishIndexAsync(workspace, options);
                    break;
                case ArtifactType.ZippedIndex:
                    await PublishIndexZippedAsync(workspace, options);
                    break;
                case ArtifactType.RepoDistribution:
                    await PublishArtifactRepoDistributionAsync(workspace, options);
                    break;
                default:
                    break;
            }
        }

        private static async Task<string> GetRepoNameFallbackAsync(XmlWorkspace workspace)
        {
            var gstDatafile = workspace.Datafiles
                .FirstOrDefault(x => x.DataKind == SourceKind.Gamesystem);
            if (gstDatafile is not null)
            {
                var gst = (GamesystemNode?)await gstDatafile.GetDataAsync();
                return gst?.Name ?? GetFallbackName();
            }
            return GetFallbackName();

            string GetFallbackName() => new DirectoryInfo(workspace.RootPath).Name;
        }

        private async Task PublishArtifactRepoDistributionAsync(IWorkspace workspace, Options options)
        {
            var distro = await workspace.CreateRepoDistributionAsync(options.RepoName, options.Url?.AbsoluteUri);
            await TryCatchLogError(
                "bsr",
                () => Path.Combine(options.Output.FullName, options.Filename + XmlFileExtensions.RepoDistribution),
                async filepath =>
                {
                    using var stream = File.OpenWrite(filepath);
                    await distro.WriteToAsync(stream);
                });
        }

        private async Task PublishIndexZippedAsync(IWorkspace workspace, Options options)
        {
            var dataIndex = await CreateIndexAsync(workspace, options);
            var datafile = DatafileInfo.Create(options.Filename + XmlFileExtensions.DataIndexZipped, dataIndex);
            await TryCatchLogError(
                datafile.Filepath,
                () => Path.Combine(options.Output.FullName, datafile.Filepath),
                datafile.WriteXmlZippedFileAsync);
        }

        private async Task PublishIndexAsync(IWorkspace workspace, Options options)
        {
            var dataIndex = await CreateIndexAsync(workspace, options);
            var datafile = DatafileInfo.Create(options.Filename + XmlFileExtensions.DataIndex, dataIndex);
            await TryCatchLogError(
                datafile.Filepath,
                () => Path.Combine(options.Output.FullName, datafile.Filepath),
                datafile.WriteXmlFileAsync);
        }

        private static async Task<DataIndexNode> CreateIndexAsync(IWorkspace workspace, Options options)
        {
            var dataIndex = await workspace.CreateDataIndexAsync(options.RepoName, options.Url?.AbsoluteUri, x => x.GetXmlZippedFilename());
            dataIndex = options.UrlOnlyIndex ? dataIndex.WithDataIndexEntries() : dataIndex;
            dataIndex = options.AdditionalUrls.Length > 0
                ? dataIndex.AddRepositoryUrls(options.AdditionalUrls.Select(x => NodeFactory.DataIndexRepositoryUrl(x.AbsoluteUri)))
                : dataIndex;
            return dataIndex;
        }

        private async Task PublishXmlZippedAsync(IWorkspace workspace, Options options)
        {
            foreach (var datafile in workspace.Datafiles.Where(x => x.DataKind.IsDataCatalogueKind()))
            {
                await TryCatchLogError(
                    datafile.Filepath,
                    () => Path.Combine(options.Output.FullName, datafile.GetXmlZippedFilename()),
                    datafile.WriteXmlZippedFileAsync);
            }
        }

        private async Task PublishXmlAsync(IWorkspace workspace, Options options)
        {
            foreach (var datafile in workspace.Datafiles.Where(x => x.DataKind.IsDataCatalogueKind()))
            {
                await TryCatchLogError(
                    datafile.Filepath,
                    () => Path.Combine(options.Output.FullName, datafile.GetXmlFilename()),
                    datafile.WriteXmlFileAsync);
            }
        }

        private async Task TryCatchLogError(
            string originalItemPath,
            Func<string> publishItemPathGetter,
            Func<string, Task> publish)
        {
            try
            {
                var publishItemPath = publishItemPathGetter();
                Log.Debug("Creating {Filepath}", publishItemPath);
                await publish(publishItemPath);
                Log.Information("Created {Filepath}", publishItemPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to publish {OriginalFilepath}", originalItemPath);
            }
        }

        private static ArtifactType ParseArtifactType(string name)
        {
            return name switch
            {
                "xml" => ArtifactType.XmlDatafiles,
                "zip" => ArtifactType.ZippedXmlDatafiles,
                "index" => ArtifactType.Index,
                "bsi" => ArtifactType.ZippedIndex,
                "bsr" => ArtifactType.RepoDistribution,
                _ => ArtifactType.None,
            };
        }
    }
}
