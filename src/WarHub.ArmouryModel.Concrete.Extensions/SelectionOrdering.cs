using NaturalSort.Extension;

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Provides canonical selection ordering matching BattleScribe/NewRecruit output.
/// Top-level force selections are sorted by category order, then by name (natural sort).
/// Child selections are sorted by name (natural sort).
/// </summary>
internal static class SelectionOrdering
{
    /// <summary>
    /// Natural-sort string comparer (ordinal, case-sensitive).
    /// "Unit 2" sorts before "Unit 10".
    /// </summary>
    internal static IComparer<string> NaturalSort { get; } = StringComparer.Ordinal.WithNaturalSort();

    /// <summary>
    /// Returns force's selections sorted in BattleScribe canonical order:
    /// primary by category order (as declared in the force's category list),
    /// secondary by effective (modifier-applied) name (natural sort),
    /// tertiary by original entry name (natural sort).
    /// </summary>
    internal static ImmutableArray<ISelectionSymbol> GetSortedSelections(IForceSymbol force)
    {
        var categoryOrder = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < force.Categories.Length; i++)
        {
            var catEntryId = force.Categories[i].SourceEntry?.Id;
            if (catEntryId is not null)
                categoryOrder[catEntryId] = i;
        }

        // Use stable LINQ sort to preserve insertion order (FIFO) for equal elements.
        return [.. force.Selections.OrderBy(x => x, new ForceSelectionComparer(categoryOrder))];
    }

    /// <summary>
    /// Returns child selections sorted by name in BattleScribe canonical order:
    /// primary by effective (modifier-applied) name (natural sort),
    /// secondary by original entry name (natural sort).
    /// </summary>
    internal static ImmutableArray<ISelectionSymbol> GetSortedChildSelections(ISelectionSymbol parent)
    {
        // Use stable LINQ sort to preserve insertion order (FIFO) for equal elements.
        return [.. parent.Selections.OrderBy(x => x, SelectionNameComparer.Instance)];
    }

    /// <summary>
    /// Returns forces sorted by name (natural sort).
    /// </summary>
    internal static ImmutableArray<IForceSymbol> GetSortedForces(IForceContainerSymbol container)
    {
        return container.Forces.Sort(ForceNameComparer.Instance);
    }

    /// <summary>
    /// Compares selections by effective name (natural sort), with original entry name as tiebreaker
    /// only when modifiers have changed at least one name.
    /// Returns 0 for equal names to preserve insertion order (FIFO) when used with a stable sort.
    /// </summary>
    internal sealed class SelectionNameComparer : IComparer<ISelectionSymbol>
    {
        public static SelectionNameComparer Instance { get; } = new();

        public int Compare(ISelectionSymbol? a, ISelectionSymbol? b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a is null) return -1;
            if (b is null) return 1;
            var cmp = NaturalSort.Compare(a.EffectiveSourceEntry.Name, b.EffectiveSourceEntry.Name);
            if (cmp != 0) return cmp;
            if (a.EffectiveSourceEntry.Name != a.SourceEntry?.Name
                || b.EffectiveSourceEntry.Name != b.SourceEntry?.Name)
            {
                cmp = NaturalSort.Compare(a.SourceEntry?.Name ?? a.Name, b.SourceEntry?.Name ?? b.Name);
                if (cmp != 0) return cmp;
            }
            // Return 0 for equal names — stable sort preserves insertion order (FIFO).
            return 0;
        }
    }

    /// <summary>
    /// Compares force-level selections by category order first, then by name.
    /// </summary>
    internal sealed class ForceSelectionComparer(Dictionary<string, int> categoryOrder) : IComparer<ISelectionSymbol>
    {
        public int Compare(ISelectionSymbol? a, ISelectionSymbol? b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a is null) return -1;
            if (b is null) return 1;
            var aCatId = a.PrimaryCategory?.SourceEntry?.Id;
            var bCatId = b.PrimaryCategory?.SourceEntry?.Id;
            int aOrder = aCatId is not null && categoryOrder.TryGetValue(aCatId, out var ao) ? ao : -1;
            int bOrder = bCatId is not null && categoryOrder.TryGetValue(bCatId, out var bo) ? bo : -1;
            var cmp = aOrder.CompareTo(bOrder);
            if (cmp != 0) return cmp;
            return SelectionNameComparer.Instance.Compare(a, b);
        }
    }

    /// <summary>
    /// Compares forces by name (natural sort).
    /// </summary>
    internal sealed class ForceNameComparer : IComparer<IForceSymbol>
    {
        public static ForceNameComparer Instance { get; } = new();

        public int Compare(IForceSymbol? a, IForceSymbol? b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a is null) return -1;
            if (b is null) return 1;
            return NaturalSort.Compare(a.Name, b.Name);
        }
    }
}
