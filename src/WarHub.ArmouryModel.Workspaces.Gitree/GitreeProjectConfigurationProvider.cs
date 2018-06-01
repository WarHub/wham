using System.Collections.Immutable;
using WarHub.ArmouryModel.ProjectModel;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    public class GitreeProjectConfigurationProvider : ProjectConfigurationProviderBase
    {
        public const string DefaultSourcePath = "src";

        protected override ProjectConfiguration CreateDefaultCore(string path)
        {
            return new ProjectConfiguration(
                CurrentToolsetVersion,
                DefaultDirectoryReferences,
                ProjectConfiguration.DefaultOutputPath,
                ProjectFormatProviderType.Gitree);
        }

        protected override ImmutableArray<SourceFolder> DefaultDirectoryReferences { get; } =
            ImmutableArray.Create(new SourceFolder(SourceFolderKind.All, DefaultSourcePath));
    }
}
