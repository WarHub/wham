using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using WarHub.ArmouryModel.ProjectSystem;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public class BattleScribeProjectConfigurationProvider : ProjectConfigurationProviderBase
    {
        public const string DefaultSourcePath = ".";

        protected override ProjectConfiguration CreateDefault(string path)
        {
            return new ProjectConfiguration(ToolsetVersion, DefaultDirectoryReferences);
        }

        protected override ImmutableArray<DirectoryReference> DefaultDirectoryReferences { get; } =
            ImmutableArray.Create(new DirectoryReference(DirectoryReferenceKind.All, DefaultSourcePath));

        protected override ProjectConfiguration SanitizeConfiguration(ProjectConfiguration raw)
        {
            return raw.Update(raw.ToolsetVersion ?? ToolsetVersion,
                !raw.SourceDirectories.IsEmpty ? raw.SourceDirectories : DefaultDirectoryReferences);
        }
    }
}
