namespace WarHub.Armoury.Model.EntryTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class MapSelectionExtensions
    {
        /// <summary>
        ///     Maps <paramref name="parentSelection" /> subselections to appropriate <see cref="IEntryNode" /> within
        ///     <paramref name="root" />.
        /// </summary>
        /// <param name="root">Tree root for origin entry of <paramref name="parentSelection" />.</param>
        /// <param name="parentSelection">Parent selection of mapped children.</param>
        /// <returns>
        ///     Mapping of every <see cref="IEntryNode" /> to a list of <see cref="ISelection" />s, which may be empty if no
        ///     [selection] was mapped.
        /// </returns>
        /// <exception cref="ArgumentNullException"> when any parameter is null.</exception>
        public static IReadOnlyDictionary<IEntryNode, List<ISelection>> MapSelections(this INode root,
            ISelection parentSelection)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (parentSelection == null) throw new ArgumentNullException(nameof(parentSelection));
            var mappedSelections =
                root.AllDescendants(node => node.Children)
                    .Where(node => node.IsEntryNode)
                    .ToDictionary(node => node.AsEntryNode, node => new List<ISelection>());
            foreach (var selection in parentSelection.Selections)
            {
                mappedSelections[root.GetFittingEntryNode(selection, parentSelection)].Add(selection);
            }
            return mappedSelections;
        }

        /// <summary>
        ///     Maps <paramref name="selectionNodeContainer" /> subselections to appropriate <see cref="IEntryNode" /> within
        ///     <paramref name="root" />.
        /// </summary>
        /// <param name="root">Tree root for <paramref name="selectionNodeContainer" />.</param>
        /// <param name="selectionNodeContainer">Parent of mapped children.</param>
        /// <returns>
        ///     Mapping of every <see cref="IEntryNode" /> to a list of <see cref="ISelection" />s, which may be empty if no
        ///     [selection] was mapped.
        /// </returns>
        /// <exception cref="ArgumentNullException"> when any parameter is null.</exception>
        public static IReadOnlyDictionary<IEntryNode, List<ISelection>> MapSelections(this INode root,
            ISelectionNodeContainer selectionNodeContainer)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (selectionNodeContainer == null) throw new ArgumentNullException(nameof(selectionNodeContainer));
            var mappedSelections =
                root.AllDescendants(node => node.Children)
                    .Where(node => node.IsEntryNode)
                    .ToDictionary(node => node.AsEntryNode, node => new List<ISelection>());
            foreach (var selection in selectionNodeContainer.Selections)
            {
                mappedSelections[root.GetFittingEntryNode(selection, null)].Add(selection);
            }
            return mappedSelections;
        }

        private static IEntryNode GetFittingEntryNode(this INode @this, ISelection selection,
            ISelection parentSelection)
        {
            return
                @this.GetLinksTarget(selection, parentSelection)
                    .GetNotLinkedEntryNode(selection.OriginEntryPath.TargetId.Value,
                        selection.OriginGroupPath.TargetId.Value);
        }

        private static INode GetLinksTarget(this INode @this, ISelection selection,
            ISelection parentSelection)
        {
            var linksSinceParent =
                selection.OriginEntryPath.Path.Skip(parentSelection?.OriginEntryPath.Path.Count ?? 0)
                    .Select(link => link.TargetId.Value)
                    .ToArray();
            return linksSinceParent.Length == 0 ? @this : @this.GetLinksTargetCore(linksSinceParent);
        }

        private static INode GetLinksTargetCore(this INode @this, IReadOnlyCollection<Guid> linkGuids)
        {
            while (true)
            {
                if (linkGuids.Count == 0)
                {
                    return @this;
                }
                @this = @this.DescendantLinkNodes().First(node => node.IsForLinkGuid(linkGuids.First()));
                linkGuids = linkGuids.Skip(1).ToArray();
            }
        }

        private static IEntryNode GetNotLinkedEntryNode(this INode @this, Guid entryGuid, Guid groupGuid)
        {
            if (@this.IsEntryNode && @this.AsEntryNode.Entry.IdValueEquals(entryGuid))
            {
                return @this.AsEntryNode;
            }
            if (@this.IsGroupNode && @this.AsGroupNode.Group.IdValueEquals(groupGuid) ||
                @this.IsRoot && groupGuid == ReservedIdentifiers.NullId)
            {
                return @this.EntryNodes.First(node => !node.IsLinkNode && node.Entry.IdValueEquals(entryGuid));
            }
            return
                @this.DescendantNotLinkGroupNodes()
                    .First(node => node.Group.IdValueEquals(groupGuid))
                    .GetNotLinkedEntryNode(entryGuid, groupGuid);
        }
    }
}
