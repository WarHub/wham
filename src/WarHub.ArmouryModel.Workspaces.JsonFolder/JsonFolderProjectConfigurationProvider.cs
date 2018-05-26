using System.Collections.Immutable;
using WarHub.ArmouryModel.ProjectModel;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    public class JsonFolderProjectConfigurationProvider : ProjectConfigurationProviderBase
    {
        public const string DefaultSourcePath = "src";

        protected override ProjectConfiguration CreateDefaultCore(string path)
        {
            return new ProjectConfiguration(
                CurrentToolsetVersion,
                DefaultDirectoryReferences,
                ProjectConfiguration.DefaultOutputPath,
                ProjectFormatProviderType.JsonFolders);
        }

        protected override ImmutableArray<SourceFolder> DefaultDirectoryReferences { get; } =
            ImmutableArray.Create(new SourceFolder(SourceFolderKind.All, DefaultSourcePath));
    }
}
