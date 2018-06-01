using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.JsonInfrastructure;
using WarHub.ArmouryModel.Source.BattleScribe;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.JsonFolder;
using WarHub.ArmouryModel.CliTool.Utilities;
using System.IO.Compression;
using System.Reflection;

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

        protected override void MainCore()
        {
            var configInfo = new AutoProjectConfigurationProvider().Create(Source);
            Log.Debug("Using configuration: {@Config}", configInfo);
            Destination = Destination ?? configInfo.Configuration.OutputPath;
            Log.Debug("Writing artifacts to: {Destination}", Destination);
            Log.Debug("Loading workspace...");
            var workspace = WorkspaceProvider.ReadWorkspaceFromConfig(configInfo);
            Log.Debug(
                "Workspace loaded. {DatafileCount} datafiles discovered.",
                workspace.Datafiles.Where(x => x.DataKind.IsDataCatalogueKind()).Count());
            foreach (var artifactType in Artifacts ?? Enumerable.Empty<PublishArtifact>())
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
            var distro = workspace.CreateRepoDistribution("repo", "repo-url");
            var filename = GetRepoDistributionFilename(distro);
            using (var stream = File.OpenWrite(filename))
            {
                distro.WriteTo(stream);
            }
            Log.Information("Created {File}", filename);
        }

        private void PublishIndexZipped(IWorkspace workspace)
        {
            var dataIndex = workspace.CreateDataIndex("repo", "repo-url");
            var filepath = Path.Combine(Destination, ProjectConfigurationExtensions.DataIndexZippedFileName);
            using (var stream = File.Create(filepath))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            using (var entryStream = archive.CreateEntry(ProjectConfigurationExtensions.DataIndexFileName).Open())
            {
                dataIndex.Serialize(entryStream);
            }
            Log.Information("Created {File}", filepath);
        }

        private void PublishIndex(IWorkspace workspace)
        {
            var dataIndex = workspace.CreateDataIndex("repo", "repo-url");
            var filepath = Path.Combine(Destination, ProjectConfigurationExtensions.DataIndexFileName);
            using (var stream = File.Create(filepath))
            {
                dataIndex.Serialize(stream);
            }
            Log.Information("Created {File}", filepath);
        }

        private void PublishXmlZipped(IWorkspace workspace)
        {
            // TODO
            Log.Error("Publishing {Artifact} is not yet supported.", PublishArtifact.ZippedXmlDatafiles);
        }

        private void PublishXml(IWorkspace workspace)
        {
            // TODO
            Log.Error("Publishing {Artifact} is not yet supported.", PublishArtifact.XmlDatafiles);
        }

        private string GetRepoDistributionFilename(RepoDistribution distribution)
        {
            return Path.Combine(Destination, new DirectoryInfo(Source).Name + XmlFileExtensions.RepoDistribution);
        }

        private class WorkspaceProvider
        {
            public static IWorkspace ReadWorkspaceFromConfig(ProjectConfigurationInfo info)
            {
                switch (info.Configuration.FormatProvider)
                {
                    case ProjectFormatProviderType.JsonFolders:
                        return JsonWorkspace.CreateFromConfigurationInfo(info);
                    case ProjectFormatProviderType.XmlCatalogues:
                        return XmlWorkspace.CreateFromConfigurationInfo(info);
                    default:
                        throw new InvalidOperationException(
                            $"Unknown {nameof(ProjectConfiguration.FormatProvider)}:" +
                            $" {info.Configuration.FormatProvider}");
                }
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
