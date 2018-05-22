using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public static class ProjectConfigurationExtensions
    {
        static ProjectConfigurationExtensions()
        {
            DirectoryReferenceKindsBySource = new Dictionary<SourceKind, ImmutableHashSet<SourceFolderKind>>
            {
                [SourceKind.Catalogue] = ImmutableHashSet.Create(SourceFolderKind.All, SourceFolderKind.Catalogues),
                [SourceKind.Gamesystem] = ImmutableHashSet.Create(SourceFolderKind.All, SourceFolderKind.Gamesystems)
            }
            .ToImmutableDictionary();
        }

        public static ImmutableDictionary<SourceKind, ImmutableHashSet<SourceFolderKind>> DirectoryReferenceKindsBySource { get; }

        public static ImmutableHashSet<SourceFolderKind> DirectoryReferenceKinds(this SourceKind sourceKind)
            => DirectoryReferenceKindsBySource[sourceKind];

        public static SourceFolder GetRefForKind(this ProjectConfiguration config, SourceFolderKind kind)
        {
            return config.SourceDirectories.Single(dir => dir.Kind == kind);
        }
    }
}
