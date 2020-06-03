﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amadevus.RecordGenerator;
using WarHub.ArmouryModel.CliTool.Utilities;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Source.XmlFormat;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.Gitree;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public partial class PublishCommand : CommandBase
    {
        internal static readonly string[] ArtifactNames = new[] { "xml", "zip", "index", "bsi", "bsr" };

        [Record]
        private partial class Options
        {
            public ImmutableArray<ArtifactType> Artifacts { get; }
            public DirectoryInfo Source { get; }
            public DirectoryInfo Output { get; }
            public Uri Url { get; }
            public ImmutableArray<Uri> AdditionalUrls { get; }
            public bool UrlOnlyIndex { get; }
            public string RepoName { get; }
            public string Filename { get; }
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
            Uri url,
            IEnumerable<Uri> additionalUrls,
            bool urlOnlyIndex,
            string repoName,
            string filename,
            string verbosity)
        {
            SetupLogger(verbosity);
            var configInfo = new AutoProjectConfigurationProvider().Create(source.FullName);
            Log.Debug("Using configuration: {@Config}", configInfo);
            if (configInfo.Configuration.FormatProvider == ProjectFormatProviderType.Gitree)
            {
                Log.Warning("Gitree feature is a Work In Progress. It may not work as expected, or at all.");
            }

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
            output ??= new DirectoryInfo(configInfo.Configuration.OutputPath);
            Log.Debug("Writing artifacts to: {Destination}", output);
            output.Create();

            Log.Debug("Loading workspace...");
            var workspace = ReadWorkspaceFromConfig(configInfo);
            Log.Debug(
                "Workspace loaded. {DatafileCount} datafiles discovered.",
                workspace.Datafiles.Count(x => x.DataKind.IsDataCatalogueKind()));

            await CheckBattleScribeVersionCompatibilityAsync(workspace);

            var resolvedRepoName = string.IsNullOrWhiteSpace(repoName) ? await GetRepoNameFallbackAsync(workspace) : repoName;
            Log.Debug("Repository name used is: {RepoName}", resolvedRepoName);

            var resolvedFilename = string.IsNullOrWhiteSpace(filename) ? source.Name : filename;

            var options = new Options.Builder
            {
                Artifacts = artifactTypes,
                Source = source,
                Output = output,
                Url = url,
                AdditionalUrls = additionalUrls?.ToImmutableArray() ?? ImmutableArray<Uri>.Empty,
                RepoName = resolvedRepoName,
                Filename = resolvedFilename,
                UrlOnlyIndex = urlOnlyIndex
            }.ToImmutable();

            foreach (var artifactType in options.Artifacts)
            {
                await CreateArtifactAsync(workspace, options, artifactType);
            }
            Log.Verbose("All done.");
        }

        private async Task CheckBattleScribeVersionCompatibilityAsync(IWorkspace workspace)
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

        private static async Task<string> GetRepoNameFallbackAsync(IWorkspace workspace)
        {
            var gst = (GamesystemNode)await workspace.Datafiles
                .FirstOrDefault(x => x.DataKind == SourceKind.Gamesystem)
                ?.GetDataAsync();
            return gst?.Name ?? workspace.Info.GetDirectoryInfo().Name;
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

        private static IWorkspace ReadWorkspaceFromConfig(ProjectConfigurationInfo info)
        {
            return info.Configuration.FormatProvider switch
            {
                ProjectFormatProviderType.Gitree => GitreeWorkspace.CreateFromConfigurationInfo(info),
                ProjectFormatProviderType.BattleScribeXml => XmlWorkspace.CreateFromConfigurationInfo(info),
                _ => throw new InvalidOperationException(
                        $"Unknown {nameof(ProjectConfiguration.FormatProvider)}:" +
                        $" {info.Configuration.FormatProvider}"),
            };
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
