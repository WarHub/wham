namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Resolves profiles and rules from an entry's resource tree (InfoLinks, InfoGroups),
/// applying modifier evaluation for characteristics and descriptions.
/// Walks the Symbol tree directly — no SourceNode traversal needed.
/// </summary>
internal static class ResourceResolver
{
    /// <summary>
    /// Resolves all effective profiles and rules for an entry, including those
    /// reached through InfoLinks and InfoGroups. Applies modifiers from the
    /// profile/rule symbol, the containing InfoLink (if any), and the containing
    /// InfoGroup (if any).
    /// </summary>
    public static (IReadOnlyList<ResolvedProfile> Profiles, IReadOnlyList<ResolvedRule> Rules)
        ResolveEffectiveResources(
            ISelectionEntryContainerSymbol entry,
            ModifierEvaluator evaluator,
            ISelectionSymbol? selection,
            IForceSymbol? force)
    {
        // For entry links, resolve through to the shared target's resources
        var resolvedEntry = entry.ReferencedEntry as ISelectionEntryContainerSymbol ?? entry;
        var ctx = new ResolveContext(evaluator, selection, force);
        var profiles = new List<ResolvedProfile>();
        var rules = new List<ResolvedRule>();

        CollectFromEntry(resolvedEntry, ctx, profiles, rules);

        return (profiles, rules);
    }

    /// <summary>
    /// Collects profiles and rules from an entry's resources, matching
    /// StateMapper's traversal order: (1) direct profiles/rules,
    /// (2) InfoLinks, (3) inline InfoGroups.
    /// </summary>
    private static void CollectFromEntry(
        ISelectionEntryContainerSymbol entry,
        ResolveContext ctx,
        List<ResolvedProfile> profiles,
        List<ResolvedRule> rules)
    {
        // Pass 1: Direct profiles
        foreach (var res in entry.Resources)
        {
            if (res is IProfileSymbol profile && !res.IsReference)
                profiles.Add(ResolveProfile(profile, linkSym: null, groupSym: null, ctx));
        }

        // Pass 2: Direct rules
        foreach (var res in entry.Resources)
        {
            if (res is IRuleSymbol rule && !res.IsReference)
                rules.Add(ResolveRule(rule, linkSym: null, groupSym: null, ctx));
        }

        // Pass 3: InfoLinks (profile links, rule links, group links)
        foreach (var res in entry.Resources)
        {
            if (!res.IsReference) continue;
            switch (res.ResourceKind)
            {
                case ResourceKind.Profile when res.ReferencedEntry is IProfileSymbol targetProfile:
                    profiles.Add(ResolveLinkProfile(targetProfile, res, ctx));
                    break;
                case ResourceKind.Rule when res.ReferencedEntry is IRuleSymbol targetRule:
                    rules.Add(ResolveLinkRule(targetRule, res, ctx));
                    break;
                case ResourceKind.Group when res.ReferencedEntry is IResourceEntrySymbol targetGroup:
                    CollectFromGroup(targetGroup, ctx, profiles, rules, visited: null, viaLink: res);
                    break;
            }
        }

        // Pass 4: Inline InfoGroups
        foreach (var res in entry.Resources)
        {
            if (res.ResourceKind == ResourceKind.Group && !res.IsReference)
                CollectFromGroup(res, ctx, profiles, rules, visited: null, viaLink: null);
        }
    }

    /// <summary>
    /// Recursively collects profiles and rules from an InfoGroup,
    /// applying modifiers from the group and any enclosing InfoLink.
    /// </summary>
    private static void CollectFromGroup(
        IResourceEntrySymbol group,
        ResolveContext ctx,
        List<ResolvedProfile> profiles,
        List<ResolvedRule> rules,
        HashSet<string>? visited,
        IResourceEntrySymbol? viaLink)
    {
        // Cycle guard
        if (group.Id is { } groupId)
        {
            visited ??= [];
            if (!visited.Add(groupId)) return;
        }

        // Direct profiles in the group
        foreach (var res in group.Resources)
        {
            if (res is IProfileSymbol profile && !res.IsReference)
                profiles.Add(ResolveProfile(profile, linkSym: viaLink, groupSym: group, ctx));
        }

        // Direct rules in the group
        foreach (var res in group.Resources)
        {
            if (res is IRuleSymbol rule && !res.IsReference)
                rules.Add(ResolveRule(rule, linkSym: viaLink, groupSym: group, ctx));
        }

        // InfoLinks within the group
        foreach (var res in group.Resources)
        {
            if (!res.IsReference) continue;
            switch (res.ResourceKind)
            {
                case ResourceKind.Profile when res.ReferencedEntry is IProfileSymbol targetProfile:
                    // Inner link replaces outer link (matches StateMapper behavior)
                    profiles.Add(ResolveLinkProfile(targetProfile, res, ctx, groupSym: group));
                    break;
                case ResourceKind.Rule when res.ReferencedEntry is IRuleSymbol targetRule:
                    rules.Add(ResolveLinkRule(targetRule, res, ctx, groupSym: group));
                    break;
                case ResourceKind.Group when res.ReferencedEntry is IResourceEntrySymbol targetGroup:
                    // Nested group link: inner link replaces outer link
                    CollectFromGroup(targetGroup, ctx, profiles, rules, visited, viaLink: res);
                    break;
            }
        }

        // Nested inline InfoGroups
        foreach (var res in group.Resources)
        {
            if (res.ResourceKind == ResourceKind.Group && !res.IsReference)
                CollectFromGroup(res, ctx, profiles, rules, visited, viaLink);
        }
    }

    private static ResolvedProfile ResolveProfile(
        IProfileSymbol profile,
        IResourceEntrySymbol? linkSym,
        IResourceEntrySymbol? groupSym,
        ResolveContext ctx)
    {
        var chars = ResolveCharacteristics(profile, linkSym, groupSym, ctx);
        var (page, pubId) = GetPageAndPublicationId(profile);
        return new ResolvedProfile(
            Name: profile.Name ?? "",
            TypeId: profile.Type?.Id,
            TypeName: profile.Type?.Name,
            Hidden: (groupSym?.IsHidden ?? false) || profile.IsHidden,
            Characteristics: chars,
            Page: page,
            PublicationId: pubId);
    }

    private static ResolvedProfile ResolveLinkProfile(
        IProfileSymbol target,
        IResourceEntrySymbol link,
        ResolveContext ctx,
        IResourceEntrySymbol? groupSym = null)
    {
        var chars = ResolveCharacteristics(target, linkSym: link, groupSym, ctx);
        // InfoLink overrides: hidden is OR'd, name overrides target if non-empty
        var hidden = link.IsHidden || (groupSym?.IsHidden ?? false) || target.IsHidden;
        var name = !string.IsNullOrEmpty(link.Name) ? link.Name : target.Name ?? "";
        // Page and publicationId always come from the TARGET
        var (page, pubId) = GetPageAndPublicationId(target);
        return new ResolvedProfile(
            Name: name,
            TypeId: target.Type?.Id,
            TypeName: target.Type?.Name,
            Hidden: hidden,
            Characteristics: chars,
            Page: page,
            PublicationId: pubId);
    }

    private static ResolvedRule ResolveRule(
        IRuleSymbol rule,
        IResourceEntrySymbol? linkSym,
        IResourceEntrySymbol? groupSym,
        ResolveContext ctx)
    {
        var desc = ApplyRuleModifiers(rule, linkSym, groupSym, ctx);
        var (page, pubId) = GetPageAndPublicationId(rule);
        return new ResolvedRule(
            Name: rule.Name ?? "",
            Description: desc,
            Hidden: (groupSym?.IsHidden ?? false) || rule.IsHidden,
            Page: page,
            PublicationId: pubId);
    }

    private static ResolvedRule ResolveLinkRule(
        IRuleSymbol target,
        IResourceEntrySymbol link,
        ResolveContext ctx,
        IResourceEntrySymbol? groupSym = null)
    {
        var desc = ApplyRuleModifiers(target, linkSym: link, groupSym, ctx);
        var hidden = link.IsHidden || (groupSym?.IsHidden ?? false) || target.IsHidden;
        var name = !string.IsNullOrEmpty(link.Name) ? link.Name : target.Name ?? "";
        var (page, pubId) = GetPageAndPublicationId(target);
        return new ResolvedRule(
            Name: name,
            Description: desc,
            Hidden: hidden,
            Page: page,
            PublicationId: pubId);
    }

    private static ImmutableArray<ResolvedCharacteristic> ResolveCharacteristics(
        IProfileSymbol profile,
        IResourceEntrySymbol? linkSym,
        IResourceEntrySymbol? groupSym,
        ResolveContext ctx)
    {
        var builder = ImmutableArray.CreateBuilder<ResolvedCharacteristic>(profile.Characteristics.Length);
        foreach (var ch in profile.Characteristics)
        {
            var value = ch.Value ?? "";
            // Apply modifiers in order: target, link, group
            value = ctx.Evaluator.GetEffectiveCharacteristic(
                (IEntrySymbol)profile, ch.Type?.Id ?? "", value, ctx.Selection, ctx.Force);
            if (linkSym is not null)
                value = ctx.Evaluator.GetEffectiveCharacteristic(
                    (IEntrySymbol)linkSym, ch.Type?.Id ?? "", value, ctx.Selection, ctx.Force);
            if (groupSym is not null)
                value = ctx.Evaluator.GetEffectiveCharacteristic(
                    (IEntrySymbol)groupSym, ch.Type?.Id ?? "", value, ctx.Selection, ctx.Force);
            builder.Add(new ResolvedCharacteristic(
                Name: ch.Name ?? "",
                TypeId: ch.Type?.Id,
                Value: value));
        }
        return builder.MoveToImmutable();
    }

    private static string ApplyRuleModifiers(
        IRuleSymbol rule,
        IResourceEntrySymbol? linkSym,
        IResourceEntrySymbol? groupSym,
        ResolveContext ctx)
    {
        var desc = rule.DescriptionText ?? "";
        desc = ctx.Evaluator.GetEffectiveRuleDescription(
            (IEntrySymbol)rule, desc, ctx.Selection, ctx.Force);
        if (linkSym is not null)
            desc = ctx.Evaluator.GetEffectiveRuleDescription(
                (IEntrySymbol)linkSym, desc, ctx.Selection, ctx.Force);
        if (groupSym is not null)
            desc = ctx.Evaluator.GetEffectiveRuleDescription(
                (IEntrySymbol)groupSym, desc, ctx.Selection, ctx.Force);
        return desc;
    }

    /// <summary>
    /// Extracts page and publicationId from a resource entry symbol.
    /// The page is stored on the SourceNode declaration (not just through PublicationReference),
    /// since BattleScribe allows page without publicationId.
    /// </summary>
    private static (string? Page, string? PublicationId) GetPageAndPublicationId(IResourceEntrySymbol entry)
    {
        if (entry is SourceDeclaredSymbol { Declaration: Source.IPublicationReferencingNode pubRef })
            return (pubRef.Page, pubRef.PublicationId);
        // Fallback to PublicationReference symbol
        var pr = entry.PublicationReference;
        return (pr?.Page, pr?.Publication?.Id);
    }

    private readonly record struct ResolveContext(
        ModifierEvaluator Evaluator,
        ISelectionSymbol? Selection,
        IForceSymbol? Force);
}
