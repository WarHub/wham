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

    /// <summary>
    /// Collects and returns effective profiles and rules from an entry's resource graph.
    /// Walks all resources (profiles, rules, info groups, info links), resolving references
    /// and applying modifiers from each symbol along the traversal path.
    /// </summary>
    public (ImmutableArray<IEffectiveProfileSymbol> Profiles, ImmutableArray<IEffectiveRuleSymbol> Rules)
        CollectEffectiveResources(
            IEntrySymbol entry,
            ISelectionSymbol? selection,
            IForceSymbol? force)
    {
        var profiles = new List<IEffectiveProfileSymbol>();
        var rules = new List<IEffectiveRuleSymbol>();
        CollectFromResources(
            entry.Resources,
            viaInfoLink: null,
            containingGroup: null,
            selection, force, visited: null, profiles, rules);
        return (profiles.ToImmutableArray(), rules.ToImmutableArray());
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

        var (effectiveProfiles, effectiveRules) = CollectEffectiveResources(entry, selection, force);
        var effectivePage = Evaluator.GetEffectivePage(entry, selection, force);

        return new EffectiveEntrySymbol(
            entry,
            name,
            hidden,
            constraintValues,
            costValues,
            effectiveCategories,
            effectivePrimary,
            effectiveProfiles,
            effectiveRules,
            effectivePage);
    }

    /// <summary>
    /// Walks the resource graph collecting profiles and rules.
    /// Tracks at most two context symbols matching BattleScribe semantics:
    /// <paramref name="viaInfoLink"/> (the link to the containing group, for characteristic modifiers only)
    /// and <paramref name="containingGroup"/> (the immediately containing group, for modifiers + hidden fallback).
    /// </summary>
    private void CollectFromResources(
        ImmutableArray<IResourceEntrySymbol> resources,
        IEntrySymbol? viaInfoLink,
        IEntrySymbol? containingGroup,
        ISelectionSymbol? selection,
        IForceSymbol? force,
        HashSet<object>? visited,
        List<IEffectiveProfileSymbol> profiles,
        List<IEffectiveRuleSymbol> rules)
    {
        foreach (var resource in resources)
        {
            switch (resource.ResourceKind)
            {
                case ResourceKind.Cost:
                case ResourceKind.Characteristic:
                case ResourceKind.Error:
                    continue;
            }

            if (resource.IsReference && resource.ReferencedEntry is { } target)
            {
                // InfoLink — behavior depends on target type
                switch (target.ResourceKind)
                {
                    case ResourceKind.Profile when target is IProfileSymbol profile:
                        // Link directly targets profile → MapProfileNodeWithOverrides semantics
                        // (link name/hidden overrides apply)
                        profiles.Add(BuildEffectiveProfile(
                            profile, link: resource, linkOverridesProfile: true,
                            containingGroup, selection, force));
                        break;

                    case ResourceKind.Rule when target is IRuleSymbol rule:
                        rules.Add(BuildEffectiveRule(
                            rule, link: resource, linkOverridesProfile: true,
                            containingGroup, selection, force));
                        break;

                    case ResourceKind.Group:
                        // Link to group → recurse. Link becomes viaInfoLink; target group becomes containingGroup.
                        visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
                        if (visited.Add(resource))
                        {
                            CollectFromResources(target.Resources,
                                viaInfoLink: resource, containingGroup: target,
                                selection, force, visited, profiles, rules);
                        }
                        break;
                }
            }
            else if (resource.ResourceKind == ResourceKind.Group)
            {
                // Inline InfoGroup → recurse. No viaInfoLink; resource becomes containingGroup.
                visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
                if (visited.Add(resource))
                {
                    CollectFromResources(resource.Resources,
                        viaInfoLink: null, containingGroup: resource,
                        selection, force, visited, profiles, rules);
                }
            }
            else if (resource.ResourceKind == ResourceKind.Profile && resource is IProfileSymbol directProfile)
            {
                // Direct profile → MapProfileNode semantics
                // (viaInfoLink applies characteristic modifiers but NOT name/hidden overrides)
                profiles.Add(BuildEffectiveProfile(
                    directProfile, link: viaInfoLink, linkOverridesProfile: false,
                    containingGroup, selection, force));
            }
            else if (resource.ResourceKind == ResourceKind.Rule && resource is IRuleSymbol directRule)
            {
                rules.Add(BuildEffectiveRule(
                    directRule, link: viaInfoLink, linkOverridesProfile: false,
                    containingGroup, selection, force));
            }
        }
    }

    /// <summary>
    /// Builds an effective profile by applying the modifier chain: profile → link → group.
    /// When <paramref name="linkOverridesProfile"/> is true (link directly targets the profile),
    /// the link's name and hidden flags override the target. When false (link targets the
    /// containing group), only characteristic modifiers from the link are applied.
    /// </summary>
    private IEffectiveProfileSymbol BuildEffectiveProfile(
        IProfileSymbol profile,
        IEntrySymbol? link,
        bool linkOverridesProfile,
        IEntrySymbol? group,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        // Build characteristics with modifier chain: profile → link → group.
        var chars = ImmutableArray.CreateBuilder<EffectiveCharacteristic>(profile.Characteristics.Length);
        foreach (var ch in profile.Characteristics)
        {
            var typeId = ch.Type?.Id ?? "";
            var value = ch.Value ?? "";
            value = Evaluator.GetEffectiveCharacteristic(profile, typeId, value, selection, force);
            if (link is not null)
                value = Evaluator.GetEffectiveCharacteristic(link, typeId, value, selection, force);
            if (group is not null)
                value = Evaluator.GetEffectiveCharacteristic(group, typeId, value, selection, force);
            chars.Add(new EffectiveCharacteristic(ch.Name ?? "", typeId, value));
        }

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

        // Page and publication always from the target profile.
        return new EffectiveProfileSymbol(
            name,
            hidden,
            profile.Type?.Id,
            profile.Type?.Name,
            profile.PublicationReference?.Page,
            profile.PublicationReference?.Publication?.Id,
            chars.MoveToImmutable());
    }

    /// <summary>
    /// Builds an effective rule. Same modifier/override semantics as profiles.
    /// </summary>
    private IEffectiveRuleSymbol BuildEffectiveRule(
        IRuleSymbol rule,
        IEntrySymbol? link,
        bool linkOverridesProfile,
        IEntrySymbol? group,
        ISelectionSymbol? selection,
        IForceSymbol? force)
    {
        // Apply description modifiers: rule → link → group.
        var desc = rule.DescriptionText;
        desc = Evaluator.GetEffectiveRuleDescription(rule, desc, selection, force);
        if (link is not null)
            desc = Evaluator.GetEffectiveRuleDescription(link, desc, selection, force);
        if (group is not null)
            desc = Evaluator.GetEffectiveRuleDescription(group, desc, selection, force);

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

        return new EffectiveRuleSymbol(
            name,
            desc,
            hidden,
            rule.PublicationReference?.Page,
            rule.PublicationReference?.Publication?.Id);
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

