using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace WarHub.ArmouryModel.Source
{
    public static class SourceExtensions
    {
        static SourceExtensions()
        {
            DataCatalogueKinds =
                ImmutableHashSet.Create(
                    SourceKind.Gamesystem,
                    SourceKind.Catalogue);

            DataIndexKinds =
                new Dictionary<SourceKind, DataIndexEntryKind>
                {
                    [SourceKind.Catalogue] = DataIndexEntryKind.Catalogue,
                    [SourceKind.Gamesystem] = DataIndexEntryKind.Gamesystem
                }
                .ToImmutableDictionary();
        }

        public static ImmutableHashSet<SourceKind> DataCatalogueKinds { get; }

        public static ImmutableDictionary<SourceKind, DataIndexEntryKind> DataIndexKinds { get; }

        public static bool IsKind(this SourceNode @this, SourceKind kind) => @this.Kind == kind;

        /// <summary>
        /// Retrieves child info for this node from Parent node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static ChildInfo? GetChildInfoFromParent(this SourceNode node)
        {
            return node.Parent?.ChildrenInfos().ElementAt(node.IndexInParent);
        }

        public static DataIndexEntryKind GetIndexEntryKindOrUnknown(this SourceKind sourceKind)
            => DataIndexKinds.TryGetValue(sourceKind, out var kind) ? kind : DataIndexEntryKind.Unknown;

        public static bool IsDataCatalogueKind(this SourceKind kind) => DataCatalogueKinds.Contains(kind);
    }
}
