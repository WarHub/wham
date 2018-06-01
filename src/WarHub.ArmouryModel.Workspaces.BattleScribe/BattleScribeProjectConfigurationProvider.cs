using System.Collections.Immutable;
using WarHub.ArmouryModel.ProjectModel;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public class BattleScribeProjectConfigurationProvider : ProjectConfigurationProviderBase
    {
        public const string DefaultSourcePath = ".";

        public override ProjectFormatProviderType ProviderType => ProjectFormatProviderType.BattleScribeXml;

        protected override ImmutableArray<SourceFolder> DefaultDirectoryReferences { get; } =
            ImmutableArray.Create(new SourceFolder(SourceFolderKind.All, DefaultSourcePath));
    }
}
