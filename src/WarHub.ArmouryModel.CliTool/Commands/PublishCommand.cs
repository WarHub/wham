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

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class PublishCommand : CommandBase
    {
        [ArgShortcut("s")]
        [ArgDescription("Directory in which to look for project file or datafiles.")]
        [DefaultValue("."), ArgExistingDirectory]
        public string Source { get; set; }

        [ArgShortcut("d")]
        [ArgDescription("Directory to save artifacts to. Overrides default read from .whamproj file.")]
        public string Destination { get; set; }

        [ArgShortcut("a")]
        [ArgDescription("Kinds of artifacts to publish to output. Defaults to RepoDistribution (.bsr).")]
        public List<PublishArtifact> Artifacts { get; set; } = new List<PublishArtifact> { PublishArtifact.RepoDistribution };

        protected override void MainCore()
        {
            var configInfo = new AutoProjectConfigurationProvider().Create(Source);
            Destination = Destination ?? configInfo.Configuration.OutputPath;
            Directory.CreateDirectory(Destination);
            var workspace = WorkspaceProvider.ReadWorkspaceFromConfig(configInfo);
            foreach (var artifactType in Artifacts)
            {
                CreateArtifact(artifactType, workspace);
            }
        }

        private void CreateArtifact(PublishArtifact artifactType, IWorkspace workspace)
        {
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
            using (var stream = File.OpenWrite(GetRepoDistributionFilename(distro)))
            {
                distro.WriteTo(stream);
            }
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
        }

        private void PublishIndex(IWorkspace workspace)
        {
            var dataIndex = workspace.CreateDataIndex("repo", "repo-url");
            var filepath = Path.Combine(Destination, ProjectConfigurationExtensions.DataIndexZippedFileName);
            using (var stream = File.Create(filepath))
            {
                dataIndex.Serialize(stream);
            }
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
    
    public enum PublishArtifact
    {
        XmlDatafiles,
        ZippedXmlDatafiles,
        Index,
        ZippedIndex,
        RepoDistribution
    }
}
