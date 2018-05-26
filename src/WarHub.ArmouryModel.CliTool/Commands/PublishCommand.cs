using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.JsonInfrastructure;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.JsonFolder;
using WarHub.ArmouryModel.CliTool.Utilities;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class PublishCommand : CommandBase
    {
        [ArgShortcut("s")]
        [ArgDescription("Directory in which to look for project file or datafiles.")]
        [DefaultValue("."), ArgExistingDirectory]
        public string Source { get; set; }

        [ArgShortcut("d")]
        [ArgDescription("Directory to save artifacts to.")]
        [DefaultValue(".")]
        public string Destination { get; set; }

        public List<PublishArtifact> Artifacts { get; set; }

        protected override void MainCore()
        {
            Directory.CreateDirectory(Destination);
            var configInfo = new AutoProjectConfigurationProvider().Create(Source);
            var workspace = WorkspaceProvider.ReadWorkspaceFromConfig(configInfo);

            // TODO switch on artifact types, output to artifact directory
            var distro = workspace.CreateRepoDistribution("repo", "repo-url");
            using (var stream = File.OpenWrite(GetRepoDistributionFilename(distro)))
            {
                distro.WriteTo(stream);
            }
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
