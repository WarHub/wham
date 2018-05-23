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
            FolderKindsBySourceKinds = new Dictionary<SourceKind, ImmutableHashSet<SourceFolderKind>>
            {
                [SourceKind.Catalogue] = ImmutableHashSet.Create(SourceFolderKind.All, SourceFolderKind.Catalogues),
                [SourceKind.Gamesystem] = ImmutableHashSet.Create(SourceFolderKind.All, SourceFolderKind.Gamesystems)
            }
            .ToImmutableDictionary();

            SourceKindsByFolderKinds = FolderKindsBySourceKinds
                .SelectMany(x => x.Value.Select(folderKind => (folderKind, sourceKind: x.Key)))
                .GroupBy(x => x.folderKind, x => x.sourceKind)
                .ToImmutableDictionary(x => x.Key, x => x.ToImmutableHashSet());
        }

        public static ImmutableDictionary<SourceKind, ImmutableHashSet<SourceFolderKind>> FolderKindsBySourceKinds { get; }

        public static ImmutableDictionary<SourceFolderKind, ImmutableHashSet<SourceKind>> SourceKindsByFolderKinds { get; }

        public static ImmutableHashSet<SourceFolderKind> FolderKinds(this SourceKind sourceKind)
            => FolderKindsBySourceKinds[sourceKind];

        public static ImmutableHashSet<SourceKind> SourceKinds(this SourceFolderKind folderKind)
            => SourceKindsByFolderKinds[folderKind];

        public static IEnumerable<SourceFolder> GetSourceFolders(this ProjectConfiguration config, SourceKind kind)
        {
            var folderKinds = kind.FolderKinds();
            return config.SourceDirectories.Where(dir => folderKinds.Contains(dir.Kind));
        }

        public static SourceFolder GetSourceFolder(this ProjectConfiguration config, SourceKind kind)
        {
            return config.GetSourceFolders(kind).First();
        }
    }
}
