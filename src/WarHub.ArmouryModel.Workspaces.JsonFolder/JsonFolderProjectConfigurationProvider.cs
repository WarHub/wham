using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using WarHub.ArmouryModel.ProjectSystem;

namespace WarHub.ArmouryModel.Workspaces.JsonFolder
{
    public class JsonFolderProjectConfigurationProvider : ProjectConfigurationProviderBase
    {
        public const string DefaultSourcePath = "src";

        protected override ProjectConfiguration CreateDefault(string path)
        {
            return new ProjectConfiguration(
                CurrentToolsetVersion,
                DefaultDirectoryReferences,
                ProjectConfiguration.DefaultOutputPath,
                ProjectFormatProviderType.JsonFolders);
        }

        protected override ImmutableArray<DirectoryReference> DefaultDirectoryReferences { get; } =
            ImmutableArray.Create(new DirectoryReference(DirectoryReferenceKind.All, DefaultSourcePath));
    }
}
