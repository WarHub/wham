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
    /// secondary by original entry name (natural sort),
    /// tertiary by effective (modifier-applied) name (natural sort).
    /// </summary>
    internal static ImmutableArray<ISelectionSymbol> GetSortedSelections(IForceSymbol force)
    {
        // Build category order map from force's category list
        var categoryOrder = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < force.Categories.Length; i++)
        {
            var catEntryId = force.Categories[i].SourceEntry?.Id;
            if (catEntryId is not null)
                categoryOrder[catEntryId] = i;
        }

        return force.Selections
            .Sort((a, b) =>
            {
                var aCatId = a.PrimaryCategory?.SourceEntry?.Id;
                var bCatId = b.PrimaryCategory?.SourceEntry?.Id;
                int aOrder = aCatId is not null && categoryOrder.TryGetValue(aCatId, out var ao) ? ao : -1;
                int bOrder = bCatId is not null && categoryOrder.TryGetValue(bCatId, out var bo) ? bo : -1;
                var cmp = aOrder.CompareTo(bOrder);
                if (cmp != 0) return cmp;
                // Sort by original (pre-modifier) entry name, with effective name as tiebreaker
                cmp = NaturalSort.Compare(a.SourceEntry?.Name ?? a.Name, b.SourceEntry?.Name ?? b.Name);
                if (cmp != 0) return cmp;
                return NaturalSort.Compare(a.EffectiveSourceEntry.Name, b.EffectiveSourceEntry.Name);
            });
    }

    /// <summary>
    /// Returns child selections sorted by name in BattleScribe canonical order:
    /// primary by original entry name (natural sort),
    /// secondary by effective name (natural sort).
    /// </summary>
    internal static ImmutableArray<ISelectionSymbol> GetSortedChildSelections(ISelectionSymbol parent)
    {
        return parent.Selections
            .Sort((a, b) =>
            {
                var cmp = NaturalSort.Compare(a.SourceEntry?.Name ?? a.Name, b.SourceEntry?.Name ?? b.Name);
                if (cmp != 0) return cmp;
                return NaturalSort.Compare(a.EffectiveSourceEntry.Name, b.EffectiveSourceEntry.Name);
            });
    }

    /// <summary>
    /// Returns forces sorted by name (natural sort).
    /// </summary>
    internal static ImmutableArray<IForceSymbol> GetSortedForces(IForceContainerSymbol container)
    {
        return container.Forces.Sort((a, b) => NaturalSort.Compare(a.Name, b.Name));
    }
}
