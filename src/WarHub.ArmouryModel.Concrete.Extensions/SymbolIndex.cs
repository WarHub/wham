namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Lazy per-compilation index for efficient <see cref="SymbolKey"/> resolution.
/// Indexes all identifiable symbols by (Kind, ContainingModuleId, SymbolId) for O(1) lookup.
/// </summary>
internal sealed class SymbolIndex
{
    private readonly Dictionary<(SymbolKind Kind, string? ModuleId, string? SymbolId), List<ISymbol>> _index;
    private readonly SymbolIndex? _catalogueIndex;

    private SymbolIndex(
        Dictionary<(SymbolKind, string?, string?), List<ISymbol>> index,
        SymbolIndex? catalogueIndex = null)
    {
        _index = index;
        _catalogueIndex = catalogueIndex;
    }

    internal static SymbolIndex Build(WhamCompilation compilation, SymbolIndex? catalogueIndex = null)
    {
        var index = new Dictionary<(SymbolKind, string?, string?), List<ISymbol>>();
        var ns = compilation.GlobalNamespace;

        if (catalogueIndex is null)
        {
            // Catalogue compilation (or standalone): index all catalogue symbols.
            foreach (var catalogue in ns.Catalogues)
            {
                IndexSymbol(index, catalogue);
                IndexCatalogueContents(index, catalogue);
            }
        }
        // else: roster compilation — catalogue symbols are resolved via catalogueIndex fallback.

        // Index roster symbols (always needed when rosters exist).
        foreach (var roster in ns.Rosters)
        {
            IndexSymbol(index, roster);
            IndexRosterContents(index, roster);
        }

        return new SymbolIndex(index, catalogueIndex);
    }

    internal SymbolKeyResolution Resolve(SymbolKey key)
    {
        var lookupKey = (key.Kind, key.ContainingModuleId, key.SymbolId);

        if (_index.TryGetValue(lookupKey, out var candidates) && candidates.Count > 0)
        {
            if (candidates.Count == 1)
            {
                return SymbolKeyResolution.Resolved(candidates[0]);
            }

            // Multiple matches — try to disambiguate via ContainingEntryId.
            if (key.ContainingEntryId is not null)
            {
                var filtered = candidates
                    .Where(s => GetContainingEntryId(s) == key.ContainingEntryId)
                    .ToList();
                if (filtered.Count == 1)
                {
                    return SymbolKeyResolution.Resolved(filtered[0]);
                }
                if (filtered.Count > 1)
                {
                    return SymbolKeyResolution.Ambiguous([.. filtered]);
                }
            }

            return SymbolKeyResolution.Ambiguous([.. candidates]);
        }

        // Fall back to catalogue index for roster compilations.
        if (_catalogueIndex is not null)
        {
            return _catalogueIndex.Resolve(key);
        }

        return SymbolKeyResolution.Missing();
    }

    private static string? GetContainingEntryId(ISymbol symbol)
    {
        for (var parent = symbol.ContainingSymbol; parent is not null; parent = parent.ContainingSymbol)
        {
            switch (parent.Kind)
            {
                case SymbolKind.Catalogue:
                case SymbolKind.Roster:
                case SymbolKind.Namespace:
                    return null;
                case SymbolKind.ContainerEntry:
                case SymbolKind.Container:
                    return parent.Id;
            }
        }
        return null;
    }

    private static void IndexSymbol(
        Dictionary<(SymbolKind, string?, string?), List<ISymbol>> index,
        ISymbol symbol)
    {
        if (symbol.Id is null)
            return;

        var key = (symbol.Kind, symbol.ContainingModule?.Id, symbol.Id);
        if (!index.TryGetValue(key, out var list))
        {
            list = [];
            index[key] = list;
        }
        list.Add(symbol);
    }

    private static void IndexCatalogueContents(
        Dictionary<(SymbolKind, string?, string?), List<ISymbol>> index,
        ICatalogueSymbol catalogue)
    {
        // Resource definitions (cost types, profile types, characteristic types).
        foreach (var resDef in catalogue.ResourceDefinitions)
        {
            IndexSymbol(index, resDef);
        }

        // Root resource entries (profiles, rules, info groups, publications).
        foreach (var resEntry in catalogue.RootResourceEntries)
        {
            IndexSymbol(index, resEntry);
        }

        // Root container entries (selection entries, force entries, category entries, links).
        foreach (var entry in catalogue.RootContainerEntries)
        {
            IndexContainerEntry(index, entry);
        }

        // Shared selection entry containers.
        foreach (var shared in catalogue.SharedSelectionEntryContainers)
        {
            IndexContainerEntry(index, shared);
        }
    }

    private static void IndexContainerEntry(
        Dictionary<(SymbolKind, string?, string?), List<ISymbol>> index,
        IContainerEntrySymbol entry)
    {
        IndexSymbol(index, entry);

        // Index child entries recursively.
        if (entry is ISelectionEntryContainerSymbol selectionContainer)
        {
            foreach (var child in selectionContainer.ChildSelectionEntries)
            {
                IndexContainerEntry(index, child);
            }

            // Index category links on selection entries.
            foreach (var category in selectionContainer.Categories)
            {
                IndexSymbol(index, category);
            }
        }

        // Index force entry children and category links.
        if (entry is IForceEntrySymbol forceEntry)
        {
            foreach (var childForce in forceEntry.ChildForces)
            {
                IndexContainerEntry(index, childForce);
            }
            foreach (var category in forceEntry.Categories)
            {
                IndexSymbol(index, category);
            }
        }

        // Index resources attached to this entry.
        if (entry is IEntrySymbol entrySymbol)
        {
            foreach (var resource in entrySymbol.Resources)
            {
                IndexSymbol(index, resource);
            }
        }
    }

    private static void IndexRosterContents(
        Dictionary<(SymbolKind, string?, string?), List<ISymbol>> index,
        IRosterSymbol roster)
    {
        foreach (var force in roster.Forces)
        {
            IndexForce(index, force);
        }

        foreach (var cost in roster.Costs)
        {
            IndexSymbol(index, cost);
        }
    }

    private static void IndexForce(
        Dictionary<(SymbolKind, string?, string?), List<ISymbol>> index,
        IForceSymbol force)
    {
        IndexSymbol(index, force);

        foreach (var category in force.Categories)
        {
            IndexSymbol(index, category);
        }

        foreach (var selection in force.Selections)
        {
            IndexSelection(index, selection);
        }

        // Nested forces.
        foreach (var childForce in force.Forces)
        {
            IndexForce(index, childForce);
        }
    }

    private static void IndexSelection(
        Dictionary<(SymbolKind, string?, string?), List<ISymbol>> index,
        ISelectionSymbol selection)
    {
        IndexSymbol(index, selection);

        // Index selection instance costs and categories.
        foreach (var cost in selection.Costs)
        {
            IndexSymbol(index, cost);
        }
        foreach (var category in selection.Categories)
        {
            IndexSymbol(index, category);
        }

        // Nested selections.
        foreach (var childSelection in selection.Selections)
        {
            IndexSelection(index, childSelection);
        }
    }
}
