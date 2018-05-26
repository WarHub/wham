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
        [ArgDescription("Directory in which to look for project file or datafiles.")]
        public string Source { get; set; }

        [ArgDescription("File or directory to save artifacts to.")]
        public string Destination { get; set; }

        public List<PublishArtifact> Artifacts { get; }

        protected override void MainCore()
        {
            var configInfo = new AutoProjectConfigurationProvider().Create(Source);
            switch (configInfo.Configuration.FormatProvider)
            {
                case ProjectFormatProviderType.JsonFolders:
                    //PublishJson();
                    break;
                case ProjectFormatProviderType.XmlCatalogues:
                    //PublishXml();
                    break;
                default:
                    Log.Error(
                        "Publishing unknown ProjectFormat: '{Format}' is not supported.",
                        configInfo.Configuration.FormatProvider);
                    break;
            }
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
