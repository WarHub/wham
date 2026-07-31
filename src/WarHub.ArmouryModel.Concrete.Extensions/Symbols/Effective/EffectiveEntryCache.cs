using System.Collections.Concurrent;

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Thread-safe cache of <see cref="EffectiveSelectionEntrySymbol"/> instances keyed by
/// (entry, selection context, force context). Lazily computes effective entries
/// on first access using an internal <see cref="ModifierEvaluator"/>.
/// <para>
/// Created per-roster and stored on <see cref="RosterSymbol"/>.
/// Since compilations are immutable, cached values are stable.
/// </para>
/// </summary>
internal sealed class EffectiveEntryCache
{
    private readonly ConcurrentDictionary<EffectiveEntryKey, EffectiveSelectionEntrySymbol> _cache = new();
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
    public EffectiveSelectionEntrySymbol GetEffectiveEntry(
        ISelectionEntryContainerSymbol entry,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        return _cache.GetOrAdd(
            new EffectiveEntryKey(entry, selection, force),
            key => CreateEffectiveEntry(key.Entry, key.Selection, key.Force));
    }

    /// <summary>
    /// Creates a fully effective force entry symbol with modifier-applied
    /// name, hidden, constraints, costs, resources, and publication reference.
    /// Caching is handled externally (e.g. by <see cref="ForceSymbol"/>).
    /// </summary>
    public EffectiveForceEntrySymbol CreateEffectiveForceEntry(
        IForceEntrySymbol entry,
        IForceSymbol? force)
    {
        var name = Evaluator.GetEffectiveName(entry, selection: null, force);
        var hidden = Evaluator.GetEffectiveHidden(entry, selection: null, force);
        var constraintValues = Evaluator.GetEffectiveConstraintValues(entry, selection: null, force);
        var effectiveResources = CollectEffectiveResources(entry, selection: null, force);
        var effectivePubRef = BuildEffectivePublicationReference(entry, selection: null, force);

        return new EffectiveForceEntrySymbol(
            entry,
            name,
            hidden,
            constraintValues,
            effectiveResources,
            effectivePubRef);
    }

    /// <summary>
    /// Collects and returns effective resources (profiles, rules, costs)
    /// from an entry's resource graph as a flat list.
    /// For entry links, resolves through to the shared target's resources.
    /// </summary>
    public ImmutableArray<IResourceEntrySymbol>
        CollectEffectiveResources(
            IEntrySymbol entry,
            ISelectionSymbol? selection,
            IForceSymbol? force)
    {
        // For entry links, resolve through to the shared target's resources
        var resolvedEntry = entry.ReferencedEntry ?? entry;
        var resources = new List<IResourceEntrySymbol>();
        CollectFromResources(
            resolvedEntry.Resources,
            viaInfoLink: null,
            containingGroup: null,
            entry, selection, force, visited: null, resources);
        return resources.ToImmutableArray();
    }

    private EffectiveSelectionEntrySymbol CreateEffectiveEntry(
        ISelectionEntryContainerSymbol entry,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        var name = Evaluator.GetEffectiveName(entry, selection, force);
        var hidden = Evaluator.GetEffectiveHidden(entry, selection, force);
        var constraintValues = Evaluator.GetEffectiveConstraintValues(entry, selection, force);

        // When a selection is present, start from the selection's runtime categories
        // (which include group-inherited categories). Otherwise use entry's declared categories.
        (List<string> CategoryIds, string? PrimaryCategoryId) catResult;
        if (selection is not null)
        {
            var initialCats = new List<string>();
            string? initialPrimary = null;
            foreach (var cat in selection.Categories)
            {
                if (cat.SourceEntry?.Id is { } catId)
                    initialCats.Add(catId);
                if (cat.IsPrimaryCategory && cat.SourceEntry?.Id is { } primId)
                    initialPrimary = primId;
            }
            catResult = Evaluator.GetEffectiveCategoriesFrom(entry, initialCats, initialPrimary, selection, force);
        }
        else
        {
            catResult = Evaluator.GetEffectiveCategories(entry, selection, force);
        }
        var (effectiveCategories, effectivePrimary) = ResolveCategorySymbols(catResult.CategoryIds, catResult.PrimaryCategoryId);

        var effectiveResources = CollectEffectiveResources(entry, selection, force);

        var effectivePubRef = BuildEffectivePublicationReference(entry, selection, force);

        return new EffectiveSelectionEntrySymbol(
            entry,
            name,
            hidden,
            constraintValues,
            effectiveResources,
            effectiveCategories,
            effectivePrimary,
            effectivePubRef);
    }

    /// <summary>
    /// Walks the resource graph collecting resources into a flat list using a 3-pass traversal:
    /// Walks the resource graph collecting effective resources in a single pass.
    /// For each resource in source order: direct profiles/rules/costs are wrapped with
    /// effective values; links resolve through to their targets; groups recurse.
    /// Tracks context symbols for modifier chains:
    /// <paramref name="viaInfoLink"/> (the link to the containing group, for characteristic modifiers only)
    /// and <paramref name="containingGroup"/> (the immediately containing group, for modifiers + hidden fallback).
    /// <paramref name="entry"/> is the top-level entry whose effects apply to cost modifiers.
    /// </summary>
    private void CollectFromResources(
        ImmutableArray<IResourceEntrySymbol> resources,
        IEntrySymbol? viaInfoLink,
        IEntrySymbol? containingGroup,
        IEntrySymbol entry,
        ISelectionSymbol? selection,
        IForceSymbol? force,
        HashSet<object>? visited,
        List<IResourceEntrySymbol> result)
    {
        foreach (var resource in resources)
        {
            if (resource.IsReference)
            {
                // Link — resolve through to target
                if (resource.ReferencedEntry is not { } target)
                    continue;

                switch (target)
                {
                    case IProfileSymbol profile:
                        result.Add(BuildEffectiveProfile(
                            profile, link: resource, linkOverridesProfile: true,
                            containingGroup, selection, force));
                        break;

                    case IRuleSymbol rule:
                        result.Add(BuildEffectiveRule(
                            rule, link: resource, linkOverridesProfile: true,
                            containingGroup, selection, force));
                        break;

                    default:
                        if (target.ResourceKind == ResourceKind.Group)
                        {
                            visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
                            if (visited.Add(resource))
                            {
                                CollectFromResources(target.Resources,
                                    viaInfoLink: resource, containingGroup: target,
                                    entry, selection, force, visited, result);
                            }
                        }
                        break;
                }
            }
            else if (resource.ResourceKind == ResourceKind.Group)
            {
                // Inline group — recurse
                visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
                if (visited.Add(resource))
                {
                    CollectFromResources(resource.Resources,
                        viaInfoLink: null, containingGroup: resource,
                        entry, selection, force, visited, result);
                }
            }
            else
            {
                // Direct resource (profile, rule, cost)
                switch (resource)
                {
                    case IProfileSymbol directProfile:
                        result.Add(BuildEffectiveProfile(
                            directProfile, link: viaInfoLink, linkOverridesProfile: false,
                            containingGroup, selection, force));
                        break;

                    case IRuleSymbol directRule:
                        result.Add(BuildEffectiveRule(
                            directRule, link: viaInfoLink, linkOverridesProfile: false,
                            containingGroup, selection, force));
                        break;

                    case ICostSymbol cost:
                        result.Add(BuildEffectiveCost(cost, entry, selection, force));
                        break;
                }
            }
        }
    }

    private IResourceEntrySymbol BuildEffectiveCost(
        ICostSymbol cost,
        IEntrySymbol entry,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        if (entry.Effects.IsEmpty)
            return cost;
        var effectiveValue = Evaluator.GetEffectiveCostValue(cost, entry, selection, force);
        if (effectiveValue != cost.Value)
            return new EffectiveCostSymbol(cost, effectiveValue);
        return cost;
    }

    /// <summary>
    /// Builds an effective profile by applying the modifier chain: profile → link → group.
    /// When <paramref name="linkOverridesProfile"/> is true (link directly targets the profile),
    /// the link's name and hidden flags override the target. When false (link targets the
    /// containing group), only characteristic modifiers from the link are applied.
    /// Returns the original profile when no modifiers change anything.
    /// </summary>
    private IProfileSymbol BuildEffectiveProfile(
        IProfileSymbol profile,
        IEntrySymbol? link,
        bool linkOverridesProfile,
        IEntrySymbol? group,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        // Determine name and hidden from link/group overrides (property-based, not effect-based).
        bool hidden;
        string name;
        if (linkOverridesProfile && link is not null)
        {
            // MapProfileNodeWithOverrides: link.Hidden || (group?.Hidden ?? profile.Hidden)
            var baseHidden = group?.IsHidden ?? profile.IsHidden;
            hidden = link.IsHidden || baseHidden;
            name = !string.IsNullOrEmpty(link.Name) ? link.Name : profile.Name ?? "";
        }
        else
        {
            // MapProfileNode: group?.Hidden ?? profile.Hidden; name from profile only
            hidden = group?.IsHidden ?? profile.IsHidden;
            name = profile.Name ?? "";
        }

        // Short-circuit: when no entry in the chain has effects, skip characteristic evaluation.
        var chainHasEffects = !profile.Effects.IsEmpty
            || (link is not null && !link.Effects.IsEmpty)
            || (group is not null && !group.Effects.IsEmpty);
        if (!chainHasEffects)
        {
            if (name == profile.Name && hidden == profile.IsHidden)
                return profile;
            return new EffectiveProfileSymbol(profile, name, hidden, profile.Characteristics);
        }

        // Build characteristics with modifier chain: profile → link → group.
        var anyCharChanged = false;
        var chars = ImmutableArray.CreateBuilder<ICharacteristicSymbol>(profile.Characteristics.Length);
        foreach (var ch in profile.Characteristics)
        {
            var typeId = ch.Type?.Id ?? "";
            var originalValue = ch.Value ?? "";
            var value = originalValue;
            value = Evaluator.GetEffectiveCharacteristic(profile, typeId, value, selection, force);
            if (link is not null)
                value = Evaluator.GetEffectiveCharacteristic(link, typeId, value, selection, force);
            if (group is not null)
                value = Evaluator.GetEffectiveCharacteristic(group, typeId, value, selection, force);
            if (value != originalValue)
            {
                anyCharChanged = true;
                chars.Add(new EffectiveCharacteristicSymbol(ch, value));
            }
            else
            {
                chars.Add(ch);
            }
        }

        if (!anyCharChanged && name == profile.Name && hidden == profile.IsHidden)
            return profile;

        return new EffectiveProfileSymbol(
            profile,
            name,
            hidden,
            anyCharChanged ? chars.MoveToImmutable() : profile.Characteristics);
    }

    /// <summary>
    /// Builds an effective rule. Same modifier/override semantics as profiles.
    /// Returns the original rule when no modifiers change anything.
    /// </summary>
    private IRuleSymbol BuildEffectiveRule(
        IRuleSymbol rule,
        IEntrySymbol? link,
        bool linkOverridesProfile,
        IEntrySymbol? group,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        // Determine name and hidden from link/group overrides (property-based, not effect-based).
        bool hidden;
        string name;
        if (linkOverridesProfile && link is not null)
        {
            var baseHidden = group?.IsHidden ?? rule.IsHidden;
            hidden = link.IsHidden || baseHidden;
            name = !string.IsNullOrEmpty(link.Name) ? link.Name : rule.Name ?? "";
        }
        else
        {
            hidden = group?.IsHidden ?? rule.IsHidden;
            name = rule.Name ?? "";
        }

        // Short-circuit: when no entry in the chain has effects, skip description evaluation.
        var chainHasEffects = !rule.Effects.IsEmpty
            || (link is not null && !link.Effects.IsEmpty)
            || (group is not null && !group.Effects.IsEmpty);
        if (!chainHasEffects)
        {
            if (name == rule.Name && hidden == rule.IsHidden)
                return rule;
            return new EffectiveRuleSymbol(rule, name, hidden, rule.DescriptionText);
        }

        // Apply description modifiers: rule → link → group.
        var desc = rule.DescriptionText;
        desc = Evaluator.GetEffectiveRuleDescription(rule, desc, selection, force);
        if (link is not null)
            desc = Evaluator.GetEffectiveRuleDescription(link, desc, selection, force);
        if (group is not null)
            desc = Evaluator.GetEffectiveRuleDescription(group, desc, selection, force);

        if (desc == rule.DescriptionText && name == rule.Name && hidden == rule.IsHidden)
            return rule;

        return new EffectiveRuleSymbol(
            rule,
            name,
            hidden,
            desc);
    }

    /// <summary>
    /// Wraps the original publication reference with an effective page if modifiers changed it,
    /// or returns the original reference as-is when page is unchanged.
    /// </summary>
    private IPublicationReferenceSymbol? BuildEffectivePublicationReference(
        IEntrySymbol entry,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        if (entry.PublicationReference is not { } originalPubRef)
            return null;
        var effectivePage = Evaluator.GetEffectivePage(entry, selection, force) ?? entry.Page;
        if (effectivePage == originalPubRef.Page)
            return originalPubRef;
        return new EffectivePublicationReferenceSymbol(originalPubRef, effectivePage);
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

