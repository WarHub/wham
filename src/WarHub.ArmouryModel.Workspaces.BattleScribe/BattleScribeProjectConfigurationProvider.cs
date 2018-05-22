using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using WarHub.ArmouryModel.ProjectModel;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public class BattleScribeProjectConfigurationProvider : ProjectConfigurationProviderBase
    {
        public const string DefaultSourcePath = ".";

        protected override ProjectConfiguration CreateDefault(string path)
        {
            return new ProjectConfiguration(
                CurrentToolsetVersion,
                DefaultDirectoryReferences,
                ProjectConfiguration.DefaultOutputPath,
                ProjectFormatProviderType.XmlCatalogues);
        }

        protected override ImmutableArray<SourceFolder> DefaultDirectoryReferences { get; } =
            ImmutableArray.Create(new SourceFolder(SourceFolderKind.All, DefaultSourcePath));
    }
}
