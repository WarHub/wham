using System.Collections.Immutable;
using WarHub.ArmouryModel.ProjectModel;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    public class GitreeProjectConfigurationProvider : ProjectConfigurationProviderBase
    {
        public const string DefaultSourcePath = "src";

        public override ProjectFormatProviderType ProviderType => ProjectFormatProviderType.Gitree;

        protected override ImmutableArray<SourceFolder> DefaultDirectoryReferences { get; } =
            ImmutableArray.Create(new SourceFolder(SourceFolderKind.All, DefaultSourcePath));
    }
}
