using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Creates and initializes <see cref="EffectiveEntryCache"/> instances backed by
/// <see cref="ModifierEvaluator"/>. Bridges the gap between the evaluation engine
/// (which computes effective values) and the symbol layer (which exposes them).
/// </summary>
internal static class EffectiveEntries
{
    /// <summary>
    /// Creates an <see cref="EffectiveEntryCache"/> that lazily computes effective entries
    /// using the given <see cref="ModifierEvaluator"/>.
    /// </summary>
    public static EffectiveEntryCache CreateCache(
        ModifierEvaluator evaluator,
        Compilation compilation)
    {
        return new EffectiveEntryCache((entry, selection, force) =>
            CreateEffectiveEntry(evaluator, compilation, entry, selection, force));
    }

    /// <summary>
    /// Initializes the <see cref="EffectiveEntryCache"/> on all roster symbols in the compilation.
    /// Safe to call multiple times (only first call takes effect per roster).
    /// </summary>
    public static void InitializeRosterCaches(
        WhamCompilation compilation,
        RosterNode roster,
        ModifierEvaluator evaluator)
    {
        var cache = CreateCache(evaluator, compilation);
        foreach (var rosterSymbol in compilation.SourceGlobalNamespace.Rosters)
        {
            if (rosterSymbol.Declaration == roster || compilation.SourceGlobalNamespace.Rosters.Length == 1)
            {
                rosterSymbol.SetEffectiveEntryCache(cache);
            }
        }
    }

    private static EffectiveEntrySymbol CreateEffectiveEntry(
        ModifierEvaluator evaluator,
        Compilation compilation,
        ISelectionEntryContainerSymbol entry,
        SelectionNode? selection,
        ForceNode? force)
    {
        var name = evaluator.GetEffectiveName(entry, selection, force);
        var hidden = evaluator.GetEffectiveHidden(entry, selection, force);
        var constraintValues = evaluator.GetEffectiveConstraintValues(entry, selection, force);
        var costValues = evaluator.GetEffectiveCosts(entry, selection, force);

        // Resolve effective categories from ModifierEvaluator
        var (effectiveCatIds, effectivePrimaryId) = evaluator.GetEffectiveCategories(entry, selection, force);
        var (effectiveCategories, effectivePrimary) = ResolveCategorySymbols(
            compilation, effectiveCatIds, effectivePrimaryId);

        return new EffectiveEntrySymbol(
            entry,
            name,
            hidden,
            constraintValues,
            costValues,
            effectiveCategories,
            effectivePrimary);
    }

    private static (ImmutableArray<ICategoryEntrySymbol> Categories, ICategoryEntrySymbol? Primary) ResolveCategorySymbols(
        Compilation compilation,
        List<string> categoryIds,
        string? primaryCategoryId)
    {
        if (categoryIds.Count == 0)
        {
            return (ImmutableArray<ICategoryEntrySymbol>.Empty, null);
        }

        // Build a lookup of all category entries across catalogues
        var categoryIndex = new Dictionary<string, ICategoryEntrySymbol>(StringComparer.Ordinal);
        foreach (var cat in compilation.GlobalNamespace.Catalogues)
        {
            IndexCategories(cat.RootContainerEntries, categoryIndex);
        }

        var builder = ImmutableArray.CreateBuilder<ICategoryEntrySymbol>(categoryIds.Count);
        ICategoryEntrySymbol? primary = null;

        foreach (var catId in categoryIds)
        {
            if (categoryIndex.TryGetValue(catId, out var catSym))
            {
                builder.Add(catSym);
                if (catId == primaryCategoryId)
                {
                    primary = catSym;
                }
            }
        }

        return (builder.ToImmutable(), primary);
    }

    private static void IndexCategories(
        ImmutableArray<IContainerEntrySymbol> entries,
        Dictionary<string, ICategoryEntrySymbol> index)
    {
        foreach (var entry in entries)
        {
            if (entry is ICategoryEntrySymbol catEntry)
            {
                var effectiveId = catEntry.ReferencedEntry?.Id ?? catEntry.Id;
                if (effectiveId is not null)
                {
                    index.TryAdd(effectiveId, catEntry);
                }
            }
        }
    }
}
