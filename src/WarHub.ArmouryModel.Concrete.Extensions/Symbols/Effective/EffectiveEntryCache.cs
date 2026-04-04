using System.Collections.Concurrent;

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Thread-safe cache of <see cref="EffectiveEntrySymbol"/> instances keyed by
/// (entry, selection context, force context). Lazily computes effective entries
/// on first access using an internal <see cref="ModifierEvaluator"/>.
/// <para>
/// Created per-roster and stored on <see cref="RosterSymbol"/>.
/// Since compilations are immutable, cached values are stable.
/// </para>
/// </summary>
internal sealed class EffectiveEntryCache
{
    private readonly ConcurrentDictionary<EffectiveEntryKey, EffectiveEntrySymbol> _cache = new();
    private readonly WhamCompilation _compilation;

    /// <summary>
    /// Creates a new cache with a <see cref="ModifierEvaluator"/> for the given roster.
    /// </summary>
    public EffectiveEntryCache(IRosterSymbol roster, WhamCompilation compilation)
    {
        _compilation = compilation;
        Evaluator = new ModifierEvaluator(roster, compilation);
    }

    /// <summary>
    /// The modifier evaluator used to compute effective values.
    /// Exposed for consumers that need direct access to evaluator methods
    /// not yet covered by effective wrapper symbols (e.g. characteristics, rules, pages).
    /// </summary>
    public ModifierEvaluator Evaluator { get; }

    /// <summary>
    /// Gets or lazily computes the effective entry for the given key.
    /// </summary>
    public EffectiveEntrySymbol GetEffectiveEntry(
        ISelectionEntryContainerSymbol entry,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        return _cache.GetOrAdd(
            new EffectiveEntryKey(entry, selection, force),
            key => CreateEffectiveEntry(key.Entry, key.Selection, key.Force));
    }

    private EffectiveEntrySymbol CreateEffectiveEntry(
        ISelectionEntryContainerSymbol entry,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        var name = Evaluator.GetEffectiveName(entry, selection, force);
        var hidden = Evaluator.GetEffectiveHidden(entry, selection, force);
        var constraintValues = Evaluator.GetEffectiveConstraintValues(entry, selection, force);
        var costValues = Evaluator.GetEffectiveCosts(entry, selection, force);

        var (effectiveCatIds, effectivePrimaryId) = Evaluator.GetEffectiveCategories(entry, selection, force);
        var (effectiveCategories, effectivePrimary) = ResolveCategorySymbols(effectiveCatIds, effectivePrimaryId);

        return new EffectiveEntrySymbol(
            entry,
            name,
            hidden,
            constraintValues,
            costValues,
            effectiveCategories,
            effectivePrimary);
    }

    private (ImmutableArray<ICategoryEntrySymbol> Categories, ICategoryEntrySymbol? Primary) ResolveCategorySymbols(
        List<string> categoryIds,
        string? primaryCategoryId)
    {
        if (categoryIds.Count == 0)
        {
            return (ImmutableArray<ICategoryEntrySymbol>.Empty, null);
        }

        var categoryIndex = new Dictionary<string, ICategoryEntrySymbol>(StringComparer.Ordinal);
        foreach (var cat in _compilation.GlobalNamespace.Catalogues)
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

    /// <summary>
    /// Resolves effective profiles and rules for an entry,
    /// including those reached through InfoLinks and InfoGroups,
    /// with modifier-applied characteristic values and rule descriptions.
    /// </summary>
    public (IReadOnlyList<ResolvedProfile> Profiles, IReadOnlyList<ResolvedRule> Rules)
        GetEffectiveResources(
            IEntrySymbol entry,
            ISelectionSymbol? selection,
            IForceSymbol? force)
    {
        return ResourceResolver.ResolveEffectiveResources(entry, Evaluator, selection, force);
    }

    /// <summary>
    /// Computes effective categories for a selection: applies entry modifiers
    /// to the selection's runtime categories and resolves category names.
    /// </summary>
    public IReadOnlyList<ResolvedCategory> GetEffectiveSelectionCategories(
        ISelectionEntryContainerSymbol entry,
        ISelectionSymbol selection,
        IForceSymbol? force)
    {
        // Read initial categories from the selection (runtime-assigned by roster engine)
        var initialCatIds = new List<string>();
        string? initialPrimaryId = null;
        var catNameMap = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var cat in selection.Categories)
        {
            // Use declaration node to avoid triggering binder for SourceEntry
            var catId = (cat is CategorySymbol concreteCat)
                ? concreteCat.Declaration.EntryId ?? ""
                : cat.SourceEntry?.Id ?? "";
            var catName = (cat is CategorySymbol concreteCat2)
                ? concreteCat2.Declaration.Name ?? ""
                : cat.SourceEntry?.Name ?? "";
            initialCatIds.Add(catId);
            catNameMap[catId] = catName;
            if (cat.IsPrimaryCategory)
                initialPrimaryId = catId;
        }

        // Apply category modifiers from the entry's effects
        var (effectiveCatIds, effectivePrimaryId) = Evaluator.GetEffectiveCategoriesFrom(
            entry, initialCatIds, initialPrimaryId, selection, force);

        // Resolve names for any modifier-added categories not in the original set
        var result = new List<ResolvedCategory>(effectiveCatIds.Count);
        foreach (var catId in effectiveCatIds)
        {
            var name = catNameMap.GetValueOrDefault(catId, "");
            if (string.IsNullOrEmpty(name))
            {
                name = FindCategoryName(catId) ?? "";
            }
            result.Add(new ResolvedCategory(name, catId, catId == effectivePrimaryId));
        }
        return result;
    }

    /// <summary>
    /// Computes effective costs for a selection: modifier-applied per-unit costs
    /// multiplied by <see cref="ISelectionSymbol.SelectedCount"/>.
    /// </summary>
    public IReadOnlyList<ResolvedCost> GetEffectiveSelectionCosts(
        EffectiveEntrySymbol effectiveEntry,
        ISelectionSymbol selection)
    {
        var result = new List<ResolvedCost>();
        foreach (var cost in effectiveEntry.Costs)
        {
            var typeId = cost.Type?.Id ?? "";
            var name = cost.Name ?? "";
            var value = (double)(cost.Value * selection.SelectedCount);
            result.Add(new ResolvedCost(name, typeId, value));
        }
        return result;
    }

    /// <summary>
    /// Resolves a publication name by ID from any catalogue in the compilation.
    /// </summary>
    public string? ResolvePublicationName(string? publicationId)
    {
        if (string.IsNullOrEmpty(publicationId))
            return null;

        foreach (var cat in _compilation.GlobalNamespace.Catalogues)
        {
            foreach (var rd in cat.ResourceDefinitions)
            {
                if (rd is IPublicationSymbol pub && pub.Id == publicationId)
                    return pub.Name;
            }
        }
        return null;
    }

    private string? FindCategoryName(string catId)
    {
        foreach (var cat in _compilation.GlobalNamespace.Catalogues)
        {
            foreach (var entry in cat.RootContainerEntries)
            {
                if (entry is ICategoryEntrySymbol catEntry)
                {
                    var effectiveId = catEntry.ReferencedEntry?.Id ?? catEntry.Id;
                    if (effectiveId == catId)
                        return catEntry.Name;
                }
            }
        }
        return null;
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
