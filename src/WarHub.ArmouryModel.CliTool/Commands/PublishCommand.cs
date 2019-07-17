﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.Utilities;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
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
            [ArgShortcut("xml")]
            XmlDatafiles,
            [ArgShortcut("zip")]
            ZippedXmlDatafiles,
            [ArgShortcut("index")]
            Index,
            [ArgShortcut("bsi")]
            ZippedIndex,
            [ArgShortcut("bsr")]
            RepoDistribution
        }

        public void Run(
            IReadOnlyCollection<string> Artifacts,
            DirectoryInfo Source,
            DirectoryInfo Output,
            Uri Url,
            IReadOnlyCollection<Uri> AdditionalUrls,
            bool UrlOnlyIndex,
            string RepoName,
            string Filename,
            string verbosity)
        {
            SetupLogger(verbosity);
            var configInfo = new AutoProjectConfigurationProvider().Create(Source.FullName);
            Log.Debug("Using configuration: {@Config}", configInfo);

            var artifactTypes = Artifacts
                .Distinct()
                .Select(ParseArtifactType)
                .Where(x => x != ArtifactType.None)
                .ToImmutableArray();
            if (artifactTypes.Length == 0)
            {
                Log.Information("Nothing to do.");
                return;
            }
            Output = Output ?? new DirectoryInfo(configInfo.Configuration.OutputPath);
            Log.Debug("Writing artifacts to: {Destination}", Output);
            Output.Create();

            Log.Debug("Loading workspace...");
            var workspace = ReadWorkspaceFromConfig(configInfo);
            Log.Debug(
                "Workspace loaded. {DatafileCount} datafiles discovered.",
                workspace.Datafiles.Where(x => x.DataKind.IsDataCatalogueKind()).Count());

            var resolvedRepoName = string.IsNullOrWhiteSpace(RepoName) ? GetRepoNameFallback(workspace) : RepoName;
            Log.Debug("Repository name used is: {RepoName}", resolvedRepoName);

            var resolvedFilename = string.IsNullOrWhiteSpace(Filename) ? Source.Name : Filename;

            var options = new Options.Builder
            {
                Artifacts = artifactTypes,
                Source = Source,
                Output = Output,
                Url = Url,
                AdditionalUrls = AdditionalUrls?.ToImmutableArray() ?? ImmutableArray<Uri>.Empty,
                RepoName = resolvedRepoName,
                Filename = resolvedFilename,
                UrlOnlyIndex = UrlOnlyIndex
            }.ToImmutable();

            foreach (var artifactType in options.Artifacts)
            {
                CreateArtifact(workspace, options, artifactType);
            }
            Log.Verbose("All done.");
        }

        private void CreateArtifact(IWorkspace workspace, Options options, ArtifactType artifactType)
        {
            Log.Information("Creating artifact: {Artifact}", artifactType);
            switch (artifactType)
            {
                case ArtifactType.XmlDatafiles:
                    PublishXml(workspace, options);
                    break;
                case ArtifactType.ZippedXmlDatafiles:
                    PublishXmlZipped(workspace, options);
                    break;
                case ArtifactType.Index:
                    PublishIndex(workspace, options);
                    break;
                case ArtifactType.ZippedIndex:
                    PublishIndexZipped(workspace, options);
                    break;
                case ArtifactType.RepoDistribution:
                    PublishArtifactRepoDistribution(workspace, options);
                    break;
                default:
                    break;
            }
        }

        private static string GetRepoNameFallback(IWorkspace workspace)
        {
            var gst = (GamesystemNode)workspace.Datafiles
                .FirstOrDefault(x => x.DataKind == SourceKind.Gamesystem)
                ?.GetData();
            return gst?.Name ?? workspace.Info.GetDirectoryInfo().Name;
        }

        private void PublishArtifactRepoDistribution(IWorkspace workspace, Options options)
        {
            var distro = workspace.CreateRepoDistribution(options.RepoName, options.Url?.AbsoluteUri);
            TryCatchLogError(
                "bsr",
                () => Path.Combine(options.Output.FullName, options.Filename + XmlFileExtensions.RepoDistribution),
                filepath =>
                {
                    using (var stream = File.OpenWrite(filepath))
                    {
                        distro.WriteTo(stream);
                    }
                });
        }

        private void PublishIndexZipped(IWorkspace workspace, Options options)
        {
            var dataIndex = CreateIndex(workspace, options);
            var datafile = DatafileInfo.Create(options.Filename + XmlFileExtensions.DataIndexZipped, dataIndex);
            TryCatchLogError(
                datafile.Filepath,
                () => Path.Combine(options.Output.FullName, datafile.Filepath),
                datafile.WriteXmlZippedFile);
        }

        private void PublishIndex(IWorkspace workspace, Options options)
        {
            var dataIndex = CreateIndex(workspace, options);
            var datafile = DatafileInfo.Create(options.Filename + XmlFileExtensions.DataIndex, dataIndex);
            TryCatchLogError(
                datafile.Filepath,
                () => Path.Combine(options.Output.FullName, datafile.Filepath),
                datafile.WriteXmlFile);
        }

        private DataIndexNode CreateIndex(IWorkspace workspace, Options options)
        {
            var dataIndex = workspace.CreateDataIndex(options.RepoName, options.Url?.AbsoluteUri, x => x.GetXmlZippedFilename());
            dataIndex = options.UrlOnlyIndex ? dataIndex.WithDataIndexEntries() : dataIndex;
            dataIndex = options.AdditionalUrls.Length > 0
                ? dataIndex.AddRepositoryUrls(options.AdditionalUrls.Select(x => NodeFactory.DataIndexRepositoryUrl(x.AbsoluteUri)))
                : dataIndex;
            return dataIndex;
        }

        private void PublishXmlZipped(IWorkspace workspace, Options options)
        {
            foreach (var datafile in workspace.Datafiles.Where(x => x.DataKind.IsDataCatalogueKind()))
            {
                TryCatchLogError(
                    datafile.Filepath,
                    () => Path.Combine(options.Output.FullName, datafile.GetXmlZippedFilename()),
                    datafile.WriteXmlZippedFile);
            }
        }

        private void PublishXml(IWorkspace workspace, Options options)
        {
            foreach (var datafile in workspace.Datafiles.Where(x => x.DataKind.IsDataCatalogueKind()))
            {
                TryCatchLogError(
                    datafile.Filepath,
                    () => Path.Combine(options.Output.FullName, datafile.GetXmlFilename()),
                    datafile.WriteXmlFile);
            }
        }

        private void TryCatchLogError(
            string originalItemPath,
            Func<string> publishItemPathGetter,
            Action<string> publish)
        {
            try
            {
                var publishItemPath = publishItemPathGetter();
                Log.Debug("Creating {Filepath}", publishItemPath);
                publish(publishItemPath);
                Log.Information("Created {Filepath}", publishItemPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to publish {OriginalFilepath}", originalItemPath);
            }
        }

        private static IWorkspace ReadWorkspaceFromConfig(ProjectConfigurationInfo info)
        {
            switch (info.Configuration.FormatProvider)
            {
                case ProjectFormatProviderType.Gitree:
                    return GitreeWorkspace.CreateFromConfigurationInfo(info);
                case ProjectFormatProviderType.BattleScribeXml:
                    return XmlWorkspace.CreateFromConfigurationInfo(info);
                default:
                    throw new InvalidOperationException(
                        $"Unknown {nameof(ProjectConfiguration.FormatProvider)}:" +
                        $" {info.Configuration.FormatProvider}");
            }
        }

        private static ArtifactType ParseArtifactType(string name)
        {
            switch (name)
            {
                case "xml": return ArtifactType.XmlDatafiles;
                case "zip": return ArtifactType.ZippedXmlDatafiles;
                case "index": return ArtifactType.Index;
                case "bsi": return ArtifactType.ZippedIndex;
                case "bsr": return ArtifactType.RepoDistribution;
                default:
                    return ArtifactType.None;
            }
        }
    }
}
