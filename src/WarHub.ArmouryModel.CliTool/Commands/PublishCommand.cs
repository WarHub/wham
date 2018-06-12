using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.Utilities;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.Gitree;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class PublishCommand : CommandBase
    {
        [ArgShortcut("a"), ArgPosition(1)]
        [ArgDefaultValue("bsr")]
        [ArgDescription(
            "Kinds of artifacts to publish to output (multiple values allowed)." +
            " Available values: xml, zip, index, bsi, bsr." +
            " XML - uncompressed cat/gst XML files;" +
            " ZIP - zipped catz/gstz XML files;" +
            " INDEX - index.xml datafile index for hosting on the web;" +
            " BSI - index.bsi zipped datafile index for hosting on the web;" +
            " BSR - zipped cat/gst datafile container with index.")]
        public List<PublishArtifact> Artifacts { get; set; }

        [ArgShortcut("s")]
        [ArgDescription("Directory in which to look for project file or datafiles.")]
        [DefaultValue("."), ArgExistingDirectory]
        public string Source { get; set; }

        [ArgShortcut("d")]
        [ArgDescription("Directory to save artifacts to. Overrides default read from .whamproj file.")]
        public string Destination { get; set; }

        [ArgShortcut("url")]
        [ArgDescription(
            "Repository url that gets included in index files" +
            " and repo distribution (.bsr).")]
        public string RepoUrl { get; set; }

        [ArgShortcut("additional-urls"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription(
            "Repository urls that will get added to index files" +
            " (doesn't impact indexes in .bsr repo distributions).")]
        public List<string> AdditionalRepositoryUrls { get; set; }

        [ArgShortcut("no-index-datafiles"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription(
            "Remove all data index entries from published indexes" +
            " (doesn't impact indexes in .bsr repo distributions).")]
        public bool NoDatafilesInIndex { get; set; }

        [ArgShortcut("name")]
        [ArgDescription(
            "Repository name that gets included in index files and repo distribution (.bsr)." +
            " By default name of the game system file is used, or parent folder name if no game system.")]
        public string RepoName { get; set; }

        [ArgShortcut("bsr-filename"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription(
            "Filename (without extension) for the repo distribution." +
            " By default parent folder name is used.")]
        public string BsrFilename { get; set; }

        [ArgShortcut("i")]
        [ArgDescription("Filename (without extension) for the index files (.xml and .bsi).")]
        [ArgDefaultValue("index")]
        public string IndexFilename { get; set; }

        protected override void MainCore()
        {
            var configInfo = new AutoProjectConfigurationProvider().Create(Source);
            Log.Debug("Using configuration: {@Config}", configInfo);
            Destination = Destination ?? configInfo.Configuration.OutputPath;
            Log.Debug("Writing artifacts to: {Destination}", Destination);
            Log.Debug("Loading workspace...");
            var workspace = ReadWorkspaceFromConfig(configInfo);
            Log.Debug(
                "Workspace loaded. {DatafileCount} datafiles discovered.",
                workspace.Datafiles.Where(x => x.DataKind.IsDataCatalogueKind()).Count());
            if (string.IsNullOrWhiteSpace(RepoName))
            {
                var gst = (GamesystemNode)workspace.Datafiles
                    .FirstOrDefault(x => x.DataKind == SourceKind.Gamesystem)
                    ?.GetData();
                RepoName = gst?.Name ?? configInfo.GetDirectoryInfo().Name;
            }
            Log.Debug("Repository name used is: {RepoName}", RepoName);

            foreach (var artifactType in Artifacts.Distinct() ?? Enumerable.Empty<PublishArtifact>())
            {
                CreateArtifact(artifactType, workspace);
            }
            Log.Verbose("All done.");
        }

        private void CreateArtifact(PublishArtifact artifactType, IWorkspace workspace)
        {
            if (artifactType == PublishArtifact.None)
            {
                return;
            }
            Directory.CreateDirectory(Destination);
            Log.Information("Creating artifact: {Artifact}", artifactType);
            switch (artifactType)
            {
                case PublishArtifact.XmlDatafiles:
                    PublishXml(workspace);
                    break;
                case PublishArtifact.ZippedXmlDatafiles:
                    PublishXmlZipped(workspace);
                    break;
                case PublishArtifact.Index:
                    PublishIndex(workspace);
                    break;
                case PublishArtifact.ZippedIndex:
                    PublishIndexZipped(workspace);
                    break;
                case PublishArtifact.RepoDistribution:
                    PublishArtifactRepoDistribution(workspace);
                    break;
                default:
                    break;
            }
        }

        private void PublishArtifactRepoDistribution(IWorkspace workspace)
        {
            var distro = workspace.CreateRepoDistribution(RepoName, RepoUrl);
            TryCatchLogError(
                "bsr",
                () => GetRepoDistributionFilepath(distro),
                filepath =>
                {
                    using (var stream = File.OpenWrite(filepath))
                    {
                        distro.WriteTo(stream);
                    }
                });
        }

        private void PublishIndexZipped(IWorkspace workspace)
        {
            var dataIndex = CreateIndex(workspace);
            var filename = !string.IsNullOrWhiteSpace(IndexFilename) ? IndexFilename : "index";
            var filepath = filename + XmlFileExtensions.DataIndexZipped;
            var datafile = DatafileInfo.Create(XmlFileExtensions.DataIndexFileName, dataIndex);
            TryCatchLogError(
                filepath,
                () => Path.Combine(Destination, filepath),
                datafile.WriteXmlZippedFile);
        }

        private void PublishIndex(IWorkspace workspace)
        {
            var dataIndex = CreateIndex(workspace);
            var filename = !string.IsNullOrWhiteSpace(IndexFilename) ? IndexFilename : "index";
            var datafile = DatafileInfo.Create(filename + XmlFileExtensions.DataIndex, dataIndex);
            TryCatchLogError(
                datafile.Filepath,
                () => Path.Combine(Destination, datafile.Filepath),
                datafile.WriteXmlFile);
        }

        private DataIndexNode CreateIndex(IWorkspace workspace)
        {
            var dataIndex = workspace.CreateDataIndex(RepoName, RepoUrl, x => x.GetXmlZippedFilename());
            dataIndex = NoDatafilesInIndex ? dataIndex.WithDataIndexEntries() : dataIndex;
            dataIndex = AdditionalRepositoryUrls?.Count > 0
                ? dataIndex.AddRepositoryUrls(AdditionalRepositoryUrls.Select(NodeFactory.DataIndexRepositoryUrl))
                : dataIndex;
            return dataIndex;
        }

        private void PublishXmlZipped(IWorkspace workspace)
        {
            foreach (var datafile in workspace.Datafiles.Where(x => x.DataKind.IsDataCatalogueKind()))
            {
                TryCatchLogError(
                    datafile.Filepath,
                    () => Path.Combine(Destination, datafile.GetXmlZippedFilename()),
                    datafile.WriteXmlZippedFile);
            }
        }

        private void PublishXml(IWorkspace workspace)
        {
            foreach (var datafile in workspace.Datafiles.Where(x => x.DataKind.IsDataCatalogueKind()))
            {
                TryCatchLogError(
                    datafile.Filepath,
                    () => Path.Combine(Destination, datafile.GetXmlFilename()),
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

        private string GetRepoDistributionFilepath(RepoDistribution distribution)
        {
            var filename = !string.IsNullOrWhiteSpace(BsrFilename) ? BsrFilename : new DirectoryInfo(Source).Name;
            return Path.Combine(Destination, filename + XmlFileExtensions.RepoDistribution);
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
    }

    public class PublishArtifactConverter
    {
        static PublishArtifactConverter()
        {
            ArtifactsByName = 
                typeof(PublishArtifact)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(x => (value: Enum.Parse<PublishArtifact>(x.Name), info: x))
                .SelectMany(
                    x => x.info
                    .GetCustomAttributes<ArgShortcut>()
                    .Select(a => a.Shortcut)
                    .Append(x.info.Name)
                    .Select(shortcut => (shortcut, x.value)))
                .ToImmutableDictionary(x => x.shortcut, x => x.value);
        }

        public static ImmutableDictionary<string, PublishArtifact> ArtifactsByName { get; }

        [ArgReviver]
        public static PublishArtifact ArtifactReviver(string name, string value)
        {
            return ArtifactsByName.TryGetValue(value, out var result)
                ? result
                : throw new ValidationArgException($"Unable to parse {value} as a {nameof(PublishArtifact)}.");
        }
    }
    
    public enum PublishArtifact
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
}
