using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.EditorServices;
using WarHub.ArmouryModel.Source;
using ProtocolRosterState = BattleScribeSpec.RosterState;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Maps the ISymbol-based roster tree (SourceNode/WhamCompilation) to BattleScribeSpec Protocol types.
/// This is the reverse of <see cref="ProtocolConverter"/> — it reads the immutable roster tree
/// and produces the Protocol state expected by the spec test harness.
/// </summary>
internal sealed class StateMapper
{
    private readonly WhamCompilation _compilation;
    private readonly EntryResolver _resolver;
    private readonly IReadOnlyList<ICatalogueSymbol> _forceCatalogues;
    private readonly EffectiveEntryCache _effectiveCache;

    // ISymbol entry lookup (entryId → ISelectionEntryContainerSymbol or IContainerEntrySymbol)
    private Dictionary<string, IEntrySymbol>? _symbolEntries;

    // Node → Symbol lookups (built lazily from the compilation's symbol tree)
    private Dictionary<ForceNode, IForceSymbol>? _forceSymbols;
    private Dictionary<SelectionNode, ISelectionSymbol>? _selectionSymbols;

    // Shared item lookups for InfoLink resolution (built lazily)
    private Dictionary<string, ProfileNode>? _sharedProfiles;
    private Dictionary<string, RuleNode>? _sharedRules;
    private Dictionary<string, InfoGroupNode>? _sharedInfoGroups;

    // Entry declaration lookup (entryId → ContainerEntryBaseNode)
    private Dictionary<string, ContainerEntryBaseNode>? _entryDeclarations;

    public StateMapper(Compilation compilation, RosterNode roster, IReadOnlyList<ICatalogueSymbol> forceCatalogues)
    {
        _compilation = (WhamCompilation)compilation;
        _resolver = new EntryResolver();
        _forceCatalogues = forceCatalogues;
        // Get or create the effective entry cache from the roster symbol.
        // The cache is self-initializing — it creates its own ModifierEvaluator.
        var rosterSymbol = _compilation.SourceGlobalNamespace.Rosters
            .FirstOrDefault(r => r.Declaration == roster)
            ?? _compilation.SourceGlobalNamespace.Rosters.FirstOrDefault();
        _effectiveCache = rosterSymbol!.GetOrCreateEffectiveEntryCache();
    }

    private void EnsureSymbolLookup()
    {
        if (_forceSymbols is not null) return;
        _forceSymbols = new Dictionary<ForceNode, IForceSymbol>();
        _selectionSymbols = new Dictionary<SelectionNode, ISelectionSymbol>();
        foreach (var rosterSym in _compilation.SourceGlobalNamespace.Rosters)
        {
            foreach (var forceSym in rosterSym.Forces)
                IndexForce(forceSym);
        }
    }

    private void IndexForce(ForceSymbol forceSym)
    {
        _forceSymbols![forceSym.Declaration] = forceSym;
        foreach (var selSym in forceSym.ChildSelections)
            IndexSelection(selSym);
        foreach (var childForce in forceSym.Forces)
            IndexForce(childForce);
    }

    private void IndexSelection(SelectionSymbol selSym)
    {
        _selectionSymbols![selSym.Declaration] = selSym;
        foreach (var childSel in selSym.ChildSelections)
            IndexSelection(childSel);
    }

    private ISelectionSymbol? LookupSelection(SelectionNode? node)
    {
        if (node is null) return null;
        EnsureSymbolLookup();
        return _selectionSymbols!.GetValueOrDefault(node);
    }

    private IForceSymbol? LookupForce(ForceNode? node)
    {
        if (node is null) return null;
        EnsureSymbolLookup();
        return _forceSymbols!.GetValueOrDefault(node);
    }

    public ProtocolRosterState MapRosterState(RosterNode roster)
    {
        var gsSym = _compilation.GlobalNamespace.RootCatalogue;

        var forces = new List<ForceState>();
        for (int i = 0; i < roster.Forces.Count; i++)
        {
            var catalogue = i < _forceCatalogues.Count ? _forceCatalogues[i] : gsSym;
            forces.Add(MapForce(roster.Forces[i], catalogue));
        }

        // Compute roster-level cost totals from effective selection costs (modifier-aware)
        var costs = ComputeRosterCostsFromSelections(roster, forces);

        // Phase 5: Constraint validation
        var errors = ConstraintValidator.Validate(roster, _compilation, _forceCatalogues);

        return new ProtocolRosterState(
            Name: roster.Name ?? "New Roster",
            GameSystemId: roster.GameSystemId ?? gsSym.Id,
            Forces: forces,
            Costs: costs,
            ValidationErrors: errors);
    }

    private ForceState MapForce(ForceNode forceNode, ICatalogueSymbol catalogue)
    {
        var selections = new List<SelectionState>();
        foreach (var selNode in forceNode.Selections)
        {
            selections.Add(MapSelection(selNode, forceNode));
        }

        var availableEntries = _resolver.GetAvailableEntries(catalogue);

        // Resolve profiles and rules from the force entry declaration
        var forceEntryDecl = FindForceEntryDeclaration(forceNode.EntryId);
        var profiles = forceEntryDecl is not null
            ? ResolveSelectionProfiles(forceEntryDecl)
            : MapNodeProfiles(forceNode.Profiles);
        var rules = forceEntryDecl is not null
            ? ResolveSelectionRules(forceEntryDecl)
            : MapNodeRules(forceNode.Rules);

        return new ForceState(
            Name: forceNode.Name ?? "",
            CatalogueId: forceNode.CatalogueId,
            Selections: selections,
            AvailableEntryCount: availableEntries.Count,
            PublicationId: forceNode.PublicationId,
            Page: forceNode.Page)
        {
            Profiles = profiles.Count > 0 ? profiles : [],
            Rules = rules.Count > 0 ? rules : [],
        };
    }

    private SelectionState MapSelection(SelectionNode selNode, ForceNode force)
    {
        var children = new List<SelectionState>();
        foreach (var childNode in selNode.Selections)
        {
            children.Add(MapSelection(childNode, force));
        }

        // Look up the ISymbol for this entry to access effects (modifiers)
        var entrySym = LookupEntrySymbol(selNode.EntryId);

        // Use effective entry from cache for modifier-applied values
        var effectiveEntry = entrySym is ISelectionEntryContainerSymbol sec
            ? _effectiveCache.GetEffectiveEntry(sec, LookupSelection(selNode), LookupForce(force))
            : null;

        var effectiveName = effectiveEntry is not null
            ? effectiveEntry.Name
            : selNode.Name ?? "";
        var effectiveHidden = effectiveEntry is not null
            ? effectiveEntry.IsHidden
            : entrySym is not null
                ? _effectiveCache.Evaluator.GetEffectiveHidden(entrySym, LookupSelection(selNode), LookupForce(force))
                : false;
        var effectiveCosts = effectiveEntry is not null
            ? GetModifiedSelectionCosts(effectiveEntry, selNode, force)
            : MapSelectionCosts(selNode);

        List<CategoryState> categories;
        if (entrySym is ISelectionEntryContainerSymbol secCat)
        {
            try
            {
                categories = GetModifiedCategories(secCat, selNode, force);
            }
            catch (InvalidCastException)
            {
                // Fall back to unmodified categories if symbol binding fails
                categories = MapSelectionCategories(selNode);
            }
        }
        else
        {
            categories = MapSelectionCategories(selNode);
        }

        // Resolve profiles and rules from the entry declaration
        var entryDecl = FindEntryDeclaration(selNode.EntryId);
        var profiles = ResolveSelectionProfiles(entryDecl, selNode, force);
        var rules = ResolveSelectionRules(entryDecl, selNode, force);

        var type = selNode.Type switch
        {
            SelectionEntryKind.Unit => "unit",
            SelectionEntryKind.Model => "model",
            SelectionEntryKind.Upgrade => "upgrade",
            _ => "upgrade",
        };

        var publicationName = ResolvePublicationName(selNode.PublicationId);

        // Apply page modifiers if entry symbol is available
        var effectivePage = selNode.Page;
        if (entrySym is not null)
        {
            var modPage = _effectiveCache.Evaluator.GetEffectivePage(entrySym, LookupSelection(selNode), LookupForce(force));
            if (modPage is not null)
                effectivePage = modPage;
        }

        return new SelectionState(
            Name: effectiveName,
            EntryId: selNode.EntryId,
            Type: type,
            Number: selNode.Number,
            Hidden: effectiveHidden,
            Costs: effectiveCosts,
            Children: children,
            Profiles: profiles.Count > 0 ? profiles : null,
            Rules: rules.Count > 0 ? rules : null,
            Categories: categories.Count > 0 ? categories : null,
            Page: effectivePage,
            PublicationId: selNode.PublicationId,
            PublicationName: publicationName);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Profile / Rule resolution from entry declarations
    // ──────────────────────────────────────────────────────────────────

    private List<ProfileState> ResolveSelectionProfiles(
        ContainerEntryBaseNode? entryDecl,
        SelectionNode? selection = null,
        ForceNode? force = null)
    {
        if (entryDecl is null) return [];

        var result = new List<ProfileState>();

        // 1. Direct profiles on the entry
        foreach (var p in entryDecl.Profiles)
        {
            result.Add(MapProfileNode(p, selection: selection, force: force));
        }

        // 2. InfoLinks pointing to profiles or infogroups
        foreach (var link in entryDecl.InfoLinks)
        {
            if (link.Type == InfoLinkKind.Profile)
            {
                var target = LookupSharedProfile(link.TargetId);
                if (target is not null)
                    result.Add(MapProfileNodeWithOverrides(target, link, selection: selection, force: force));
            }
            else if (link.Type == InfoLinkKind.InfoGroup)
            {
                var group = LookupSharedInfoGroup(link.TargetId);
                if (group is not null)
                    CollectInfoGroupProfiles(group, result, visited: null, selection: selection, force: force,
                        viaInfoLink: link);
            }
        }

        // 3. Inline InfoGroups
        foreach (var group in entryDecl.InfoGroups)
        {
            CollectInfoGroupProfiles(group, result, visited: null, selection: selection, force: force);
        }

        return result;
    }

    private List<RuleState> ResolveSelectionRules(
        ContainerEntryBaseNode? entryDecl,
        SelectionNode? selection = null,
        ForceNode? force = null)
    {
        if (entryDecl is null) return [];

        var result = new List<RuleState>();

        // 1. Direct rules
        foreach (var r in entryDecl.Rules)
        {
            result.Add(MapRuleNode(r, selection: selection, force: force));
        }

        // 2. InfoLinks pointing to rules or infogroups
        foreach (var link in entryDecl.InfoLinks)
        {
            if (link.Type == InfoLinkKind.Rule)
            {
                var target = LookupSharedRule(link.TargetId);
                if (target is not null)
                    result.Add(MapRuleNodeWithOverrides(target, link, selection: selection, force: force));
            }
            else if (link.Type == InfoLinkKind.InfoGroup)
            {
                var group = LookupSharedInfoGroup(link.TargetId);
                if (group is not null)
                    CollectInfoGroupRules(group, result, visited: null, selection: selection, force: force,
                        viaInfoLink: link);
            }
        }

        // 3. Inline InfoGroups
        foreach (var group in entryDecl.InfoGroups)
        {
            CollectInfoGroupRules(group, result, visited: null, selection: selection, force: force);
        }

        return result;
    }

    private void CollectInfoGroupProfiles(InfoGroupNode group, List<ProfileState> result, HashSet<string>? visited,
        SelectionNode? selection = null, ForceNode? force = null, InfoLinkNode? viaInfoLink = null)
    {
        var groupId = group.Id;
        if (groupId is not null)
        {
            visited ??= [];
            if (!visited.Add(groupId)) return;
        }

        // Direct profiles in the group
        foreach (var p in group.Profiles)
        {
            result.Add(MapProfileNode(p, group, selection: selection, force: force, viaInfoLink: viaInfoLink));
        }

        // InfoLinks within the group
        foreach (var link in group.InfoLinks)
        {
            if (link.Type == InfoLinkKind.Profile)
            {
                var target = LookupSharedProfile(link.TargetId);
                if (target is not null)
                    result.Add(MapProfileNodeWithOverrides(target, link, group, selection: selection, force: force));
            }
            else if (link.Type == InfoLinkKind.InfoGroup)
            {
                var nested = LookupSharedInfoGroup(link.TargetId);
                if (nested is not null)
                    CollectInfoGroupProfiles(nested, result, visited, selection: selection, force: force,
                        viaInfoLink: link);
            }
        }

        // Nested inline InfoGroups
        foreach (var nested in group.InfoGroups)
        {
            CollectInfoGroupProfiles(nested, result, visited, selection: selection, force: force);
        }
    }

    private void CollectInfoGroupRules(InfoGroupNode group, List<RuleState> result, HashSet<string>? visited,
        SelectionNode? selection = null, ForceNode? force = null, InfoLinkNode? viaInfoLink = null)
    {
        var groupId = group.Id;
        if (groupId is not null)
        {
            visited ??= [];
            if (!visited.Add(groupId)) return;
        }

        foreach (var r in group.Rules)
        {
            result.Add(MapRuleNode(r, group, selection: selection, force: force, viaInfoLink: viaInfoLink));
        }

        foreach (var link in group.InfoLinks)
        {
            if (link.Type == InfoLinkKind.Rule)
            {
                var target = LookupSharedRule(link.TargetId);
                if (target is not null)
                    result.Add(MapRuleNodeWithOverrides(target, link, group, selection: selection, force: force));
            }
            else if (link.Type == InfoLinkKind.InfoGroup)
            {
                var nested = LookupSharedInfoGroup(link.TargetId);
                if (nested is not null)
                    CollectInfoGroupRules(nested, result, visited, selection: selection, force: force,
                        viaInfoLink: link);
            }
        }

        foreach (var nested in group.InfoGroups)
        {
            CollectInfoGroupRules(nested, result, visited, selection: selection, force: force);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Profile / Rule node → Protocol state mapping
    // ──────────────────────────────────────────────────────────────────

    private ProfileState MapProfileNode(ProfileNode p, InfoGroupNode? group = null,
        SelectionNode? selection = null, ForceNode? force = null,
        InfoLinkNode? viaInfoLink = null)
    {
        // Look up the profile's ISymbol to access its modifiers
        var profileSym = LookupEntrySymbol(p.Id);
        // Also check InfoLink's modifiers if accessed via a link
        var linkSym = viaInfoLink is not null ? LookupEntrySymbol(viaInfoLink.Id) : null;
        // And InfoGroup's modifiers
        var groupSym = group is not null ? LookupEntrySymbol(group.Id) : null;

        var chars = new List<CharacteristicState>();
        foreach (var ch in p.Characteristics)
        {
            var value = ch.Value ?? "";
            // Apply modifiers from the profile itself
            if (profileSym is not null)
                value = _effectiveCache.Evaluator.GetEffectiveCharacteristic(profileSym, ch.TypeId ?? "", value, LookupSelection(selection), LookupForce(force));
            // Apply modifiers from the infolink (if linked)
            if (linkSym is not null)
                value = _effectiveCache.Evaluator.GetEffectiveCharacteristic(linkSym, ch.TypeId ?? "", value, LookupSelection(selection), LookupForce(force));
            // Apply modifiers from the infogroup
            if (groupSym is not null)
                value = _effectiveCache.Evaluator.GetEffectiveCharacteristic(groupSym, ch.TypeId ?? "", value, LookupSelection(selection), LookupForce(force));
            chars.Add(new CharacteristicState(
                Name: ch.Name ?? "",
                TypeId: ch.TypeId ?? "",
                Value: value));
        }

        return new ProfileState(
            Name: p.Name ?? "",
            TypeId: p.TypeId,
            TypeName: p.TypeName,
            Hidden: group?.Hidden ?? p.Hidden,
            Characteristics: chars,
            Page: p.Page,
            PublicationId: p.PublicationId);
    }

    private ProfileState MapProfileNodeWithOverrides(ProfileNode target, InfoLinkNode link,
        InfoGroupNode? group = null, SelectionNode? selection = null, ForceNode? force = null)
    {
        // Look up symbols for modifier evaluation
        var profileSym = LookupEntrySymbol(target.Id);
        var linkSym = LookupEntrySymbol(link.Id);
        var groupSym = group is not null ? LookupEntrySymbol(group.Id) : null;

        var chars = new List<CharacteristicState>();
        foreach (var ch in target.Characteristics)
        {
            var value = ch.Value ?? "";
            // Apply modifiers from the profile itself
            if (profileSym is not null)
                value = _effectiveCache.Evaluator.GetEffectiveCharacteristic(profileSym, ch.TypeId ?? "", value, LookupSelection(selection), LookupForce(force));
            // Apply modifiers from the infolink
            if (linkSym is not null)
                value = _effectiveCache.Evaluator.GetEffectiveCharacteristic(linkSym, ch.TypeId ?? "", value, LookupSelection(selection), LookupForce(force));
            // Apply modifiers from the infogroup
            if (groupSym is not null)
                value = _effectiveCache.Evaluator.GetEffectiveCharacteristic(groupSym, ch.TypeId ?? "", value, LookupSelection(selection), LookupForce(force));
            chars.Add(new CharacteristicState(
                Name: ch.Name ?? "",
                TypeId: ch.TypeId ?? "",
                Value: value));
        }

        // InfoLink overrides: hidden (OR'd), name overrides target if non-empty.
        // Page and publicationId always come from the TARGET, never the InfoLink.
        var hidden = link.Hidden || (group?.Hidden ?? target.Hidden);
        var name = !string.IsNullOrEmpty(link.Name) ? link.Name : target.Name ?? "";

        return new ProfileState(
            Name: name,
            TypeId: target.TypeId,
            TypeName: target.TypeName,
            Hidden: hidden,
            Characteristics: chars,
            Page: target.Page,
            PublicationId: target.PublicationId);
    }

    private RuleState MapRuleNode(RuleNode r, InfoGroupNode? group = null,
        SelectionNode? selection = null, ForceNode? force = null,
        InfoLinkNode? viaInfoLink = null)
    {
        var desc = r.Description ?? "";
        // Look up rule symbol for modifiers
        var ruleSym = LookupEntrySymbol(r.Id);
        var linkSym = viaInfoLink is not null ? LookupEntrySymbol(viaInfoLink.Id) : null;
        var groupSym = group is not null ? LookupEntrySymbol(group.Id) : null;
        if (ruleSym is not null)
            desc = _effectiveCache.Evaluator.GetEffectiveRuleDescription(ruleSym, desc, LookupSelection(selection), LookupForce(force));
        if (linkSym is not null)
            desc = _effectiveCache.Evaluator.GetEffectiveRuleDescription(linkSym, desc, LookupSelection(selection), LookupForce(force));
        if (groupSym is not null)
            desc = _effectiveCache.Evaluator.GetEffectiveRuleDescription(groupSym, desc, LookupSelection(selection), LookupForce(force));

        return new RuleState(
            Name: r.Name ?? "",
            Description: desc,
            Hidden: group?.Hidden ?? r.Hidden,
            Page: r.Page,
            PublicationId: r.PublicationId);
    }

    private RuleState MapRuleNodeWithOverrides(RuleNode target, InfoLinkNode link,
        InfoGroupNode? group = null, SelectionNode? selection = null, ForceNode? force = null)
    {
        var desc = target.Description ?? "";
        var ruleSym = LookupEntrySymbol(target.Id);
        var linkSym = LookupEntrySymbol(link.Id);
        var groupSym = group is not null ? LookupEntrySymbol(group.Id) : null;
        if (ruleSym is not null)
            desc = _effectiveCache.Evaluator.GetEffectiveRuleDescription(ruleSym, desc, LookupSelection(selection), LookupForce(force));
        if (linkSym is not null)
            desc = _effectiveCache.Evaluator.GetEffectiveRuleDescription(linkSym, desc, LookupSelection(selection), LookupForce(force));
        if (groupSym is not null)
            desc = _effectiveCache.Evaluator.GetEffectiveRuleDescription(groupSym, desc, LookupSelection(selection), LookupForce(force));

        // InfoLink overrides: hidden (OR'd), name overrides target if non-empty.
        // Page and publicationId always come from the TARGET, never the InfoLink.
        var hidden = link.Hidden || (group?.Hidden ?? target.Hidden);
        var name = !string.IsNullOrEmpty(link.Name) ? link.Name : target.Name ?? "";

        return new RuleState(
            Name: name,
            Description: desc,
            Hidden: hidden,
            Page: target.Page,
            PublicationId: target.PublicationId);
    }

    private static List<ProfileState> MapNodeProfiles(ListNode<ProfileNode> profiles)
    {
        var result = new List<ProfileState>();
        foreach (var p in profiles)
        {
            var chars = new List<CharacteristicState>();
            foreach (var ch in p.Characteristics)
            {
                chars.Add(new CharacteristicState(
                    Name: ch.Name ?? "",
                    TypeId: ch.TypeId ?? "",
                    Value: ch.Value ?? ""));
            }
            result.Add(new ProfileState(
                Name: p.Name ?? "",
                TypeId: p.TypeId,
                TypeName: p.TypeName,
                Hidden: p.Hidden,
                Characteristics: chars,
                Page: p.Page,
                PublicationId: p.PublicationId));
        }
        return result;
    }

    private static List<RuleState> MapNodeRules(ListNode<RuleNode> rules)
    {
        var result = new List<RuleState>();
        foreach (var r in rules)
        {
            result.Add(new RuleState(
                Name: r.Name ?? "",
                Description: r.Description ?? "",
                Hidden: r.Hidden,
                Page: r.Page,
                PublicationId: r.PublicationId));
        }
        return result;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Category mapping
    // ──────────────────────────────────────────────────────────────────

    private List<CategoryState> GetModifiedCategories(
        ISelectionEntryContainerSymbol entrySym,
        SelectionNode selNode,
        ForceNode force)
    {
        // Build initial category list from the runtime selection node (already resolved by engine)
        var initialCatIds = new List<string>();
        string? initialPrimaryId = null;
        var catNameMap = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var catNode in selNode.Categories)
        {
            var catId = catNode.EntryId ?? "";
            initialCatIds.Add(catId);
            catNameMap[catId] = catNode.Name ?? "";
            if (catNode.Primary)
                initialPrimaryId = catId;
        }

        // Apply category modifiers from entry symbol effects
        var (effectiveCatIds, effectivePrimaryId) = _effectiveCache.Evaluator.GetEffectiveCategoriesFrom(
            entrySym, initialCatIds, initialPrimaryId, LookupSelection(selNode), LookupForce(force));

        // Try to get names for any new categories added by modifiers
        foreach (var catId in effectiveCatIds)
        {
            if (!catNameMap.ContainsKey(catId))
            {
                var catSym = LookupEntrySymbol(catId);
                if (catSym is not null)
                    catNameMap[catId] = catSym.Name ?? "";
            }
        }

        var categories = new List<CategoryState>();
        foreach (var catId in effectiveCatIds)
        {
            var name = catNameMap.GetValueOrDefault(catId, "");
            var isPrimary = catId == effectivePrimaryId;
            categories.Add(new CategoryState(Name: name, EntryId: catId, Primary: isPrimary));
        }
        return categories;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Cost mapping
    // ──────────────────────────────────────────────────────────────────

    private static List<CostState> MapSelectionCosts(SelectionNode selNode)
    {
        var costs = new List<CostState>();
        foreach (var costNode in selNode.Costs)
        {
            var value = (double)(costNode.Value * selNode.Number);
            costs.Add(new CostState(
                Name: costNode.Name ?? "",
                TypeId: costNode.TypeId ?? "",
                Value: value));
        }
        return costs;
    }

    private static List<CategoryState> MapSelectionCategories(SelectionNode selNode)
    {
        var categories = new List<CategoryState>();
        foreach (var catNode in selNode.Categories)
        {
            categories.Add(new CategoryState(
                Name: catNode.Name ?? "",
                EntryId: catNode.EntryId,
                Primary: catNode.Primary));
        }
        return categories;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Modifier-aware cost mapping
    // ──────────────────────────────────────────────────────────────────

    private List<CostState> GetModifiedSelectionCosts(
        ISelectionEntryContainerSymbol effectiveEntry,
        SelectionNode selNode,
        ForceNode force)
    {
        // Build effective per-unit cost dictionary from the effective entry's costs
        var effectiveCosts = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var cost in effectiveEntry.Costs)
        {
            if (cost.Type?.Id is { } typeId)
            {
                effectiveCosts[typeId] = cost.Value;
            }
        }

        var result = new List<CostState>();

        // Map costs using the effective values
        foreach (var cost in selNode.Costs)
        {
            var typeId = cost.TypeId ?? "";
            if (effectiveCosts.TryGetValue(typeId, out var modifiedPerUnit))
            {
                // Use modified per-unit value times the selection count
                result.Add(new CostState(
                    Name: cost.Name ?? "",
                    TypeId: typeId,
                    Value: (double)(modifiedPerUnit * selNode.Number)));
            }
            else
            {
                // Cost type not on entry (shouldn't happen, but be safe)
                result.Add(new CostState(
                    Name: cost.Name ?? "",
                    TypeId: typeId,
                    Value: (double)cost.Value));
            }
        }
        return result;
    }

    // ──────────────────────────────────────────────────────────────────
    //  ISymbol entry lookup
    // ──────────────────────────────────────────────────────────────────

    private IEntrySymbol? LookupEntrySymbol(string? entryId)
    {
        if (string.IsNullOrEmpty(entryId)) return null;
        EnsureSymbolEntryLookup();
        return _symbolEntries!.GetValueOrDefault(entryId);
    }

    private void EnsureSymbolEntryLookup()
    {
        if (_symbolEntries is not null) return;
        _symbolEntries = new(StringComparer.Ordinal);

        foreach (var catalogue in _compilation.GlobalNamespace.Catalogues)
        {
            IndexSymbolEntries(catalogue);
        }
        IndexSymbolEntries(_compilation.GlobalNamespace.RootCatalogue);
    }

    private void IndexSymbolEntries(ICatalogueSymbol catalogue)
    {
        foreach (var entry in catalogue.RootContainerEntries)
        {
            if (entry is ISelectionEntryContainerSymbol sec)
                IndexSymbolEntryRecursive(sec);
            else if (entry.Id is not null)
                _symbolEntries![entry.Id] = entry;
        }
        foreach (var entry in catalogue.SharedSelectionEntryContainers)
        {
            IndexSymbolEntryRecursive(entry);
        }
        // Index shared profiles and rules as IEntrySymbol (for modifier lookup)
        foreach (var res in catalogue.SharedResourceEntries)
        {
            if (res.Id is not null)
                _symbolEntries!.TryAdd(res.Id, res);
            // If profile, also index characteristics
            if (res is IProfileSymbol profile)
            {
                foreach (var ch in profile.Characteristics)
                {
                    if (ch.Id is not null)
                        _symbolEntries!.TryAdd(ch.Id, ch);
                }
            }
        }
        // Index root resource entries (profiles at catalogue root)
        foreach (var res in catalogue.RootResourceEntries)
        {
            if (res.Id is not null)
                _symbolEntries!.TryAdd(res.Id, res);
            if (res is IProfileSymbol profile)
            {
                foreach (var ch in profile.Characteristics)
                {
                    if (ch.Id is not null)
                        _symbolEntries!.TryAdd(ch.Id, ch);
                }
            }
        }
        // Note: We don't iterate AllItems as it may trigger lazy binding that crashes.
        // The above indexing should cover all needed entries.
    }

    private void IndexSymbolEntryRecursive(ISelectionEntryContainerSymbol entry)
    {
        if (entry.Id is not null)
            _symbolEntries![entry.Id] = entry;
        // Index sub-entries (profiles, rules, infolinks, infogroups) from this entry's resources
        IndexResourceEntriesRecursive(entry.Resources);
        foreach (var child in entry.ChildSelectionEntries)
        {
            IndexSymbolEntryRecursive(child);
        }
    }

    private void IndexResourceEntriesRecursive(ImmutableArray<IResourceEntrySymbol> resources)
    {
        foreach (var res in resources)
        {
            if (res.Id is not null)
                _symbolEntries!.TryAdd(res.Id, res);
            // If profile, also index characteristics
            if (res is IProfileSymbol profile)
            {
                foreach (var ch in profile.Characteristics)
                {
                    if (ch.Id is not null)
                        _symbolEntries!.TryAdd(ch.Id, ch);
                }
            }
            // Recursively index resources within InfoGroups
            if (res.Resources.Length > 0)
            {
                IndexResourceEntriesRecursive(res.Resources);
            }
        }
    }

    private List<CostState> MapRosterCosts(RosterNode roster)
    {
        // Collect cost types referenced by any available entry in any force's catalogue
        var referencedTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var catalogue in _forceCatalogues)
        {
            var entries = _resolver.GetAvailableEntries(catalogue);
            foreach (var entry in entries)
            {
                CollectReferencedCostTypes(entry.Symbol, referencedTypes);
            }
        }

        var result = new List<CostState>();
        foreach (var cost in roster.Costs)
        {
            if (referencedTypes.Contains(cost.TypeId ?? ""))
            {
                result.Add(new CostState(
                    Name: cost.Name ?? "",
                    TypeId: cost.TypeId ?? "",
                    Value: (double)cost.Value));
            }
        }
        return result;
    }

    private List<CostState> ComputeRosterCostsFromSelections(RosterNode roster, List<ForceState> mappedForces)
    {
        // Collect cost types referenced by any available entry
        var referencedTypes = new HashSet<string>(StringComparer.Ordinal);
        var costNames = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var catalogue in _forceCatalogues)
        {
            var entries = _resolver.GetAvailableEntries(catalogue);
            foreach (var entry in entries)
            {
                CollectReferencedCostTypes(entry.Symbol, referencedTypes);
            }
        }
        // Collect cost type names from the roster node
        foreach (var cost in roster.Costs)
        {
            if (cost.TypeId is not null && cost.Name is not null)
                costNames[cost.TypeId] = cost.Name;
        }

        // Sum costs from all mapped selections (which have effective/modified values)
        var totals = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var force in mappedForces)
        {
            AggregateCostsFromSelections(force.Selections, totals);
        }

        // Build result, only including referenced cost types
        var result = new List<CostState>();
        foreach (var cost in roster.Costs)
        {
            var typeId = cost.TypeId ?? "";
            if (referencedTypes.Contains(typeId))
            {
                result.Add(new CostState(
                    Name: cost.Name ?? "",
                    TypeId: typeId,
                    Value: totals.GetValueOrDefault(typeId, 0)));
            }
        }
        return result;
    }

    private static void AggregateCostsFromSelections(IReadOnlyList<SelectionState> selections, Dictionary<string, double> totals)
    {
        foreach (var sel in selections)
        {
            foreach (var cost in sel.Costs)
            {
                totals.TryGetValue(cost.TypeId, out var current);
                totals[cost.TypeId] = current + cost.Value;
            }
            if (sel.Children is { Count: > 0 })
            {
                AggregateCostsFromSelections(sel.Children, totals);
            }
        }
    }

    private static void CollectReferencedCostTypes(ISelectionEntryContainerSymbol symbol, HashSet<string> types)
    {
        foreach (var cost in symbol.Costs)
        {
            if (cost.Type?.Id is { } typeId)
                types.Add(typeId);
        }
        foreach (var child in symbol.ChildSelectionEntries)
        {
            CollectReferencedCostTypes(child, types);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Shared item lookups (lazily built from all source trees)
    // ──────────────────────────────────────────────────────────────────

    private ProfileNode? LookupSharedProfile(string? targetId)
    {
        if (string.IsNullOrEmpty(targetId)) return null;
        EnsureSharedLookups();
        return _sharedProfiles!.GetValueOrDefault(targetId);
    }

    private RuleNode? LookupSharedRule(string? targetId)
    {
        if (string.IsNullOrEmpty(targetId)) return null;
        EnsureSharedLookups();
        return _sharedRules!.GetValueOrDefault(targetId);
    }

    private InfoGroupNode? LookupSharedInfoGroup(string? targetId)
    {
        if (string.IsNullOrEmpty(targetId)) return null;
        EnsureSharedLookups();
        return _sharedInfoGroups!.GetValueOrDefault(targetId);
    }

    private void EnsureSharedLookups()
    {
        if (_sharedProfiles is not null) return;

        _sharedProfiles = new(StringComparer.Ordinal);
        _sharedRules = new(StringComparer.Ordinal);
        _sharedInfoGroups = new(StringComparer.Ordinal);

        foreach (var tree in _compilation.SourceTrees)
        {
            var root = tree.GetRoot();
            if (root is GamesystemNode gs)
            {
                IndexSharedItems(gs.SharedProfiles, gs.SharedRules, gs.SharedInfoGroups);
                // Root-level rules can also be InfoLink targets
                IndexRootRules(gs.Rules);
            }
            else if (root is CatalogueNode cat)
            {
                IndexSharedItems(cat.SharedProfiles, cat.SharedRules, cat.SharedInfoGroups);
                // Root-level rules can also be InfoLink targets
                IndexRootRules(cat.Rules);
            }
        }
    }

    private void IndexSharedItems(
        ListNode<ProfileNode> profiles,
        ListNode<RuleNode> rules,
        ListNode<InfoGroupNode> infoGroups)
    {
        foreach (var p in profiles)
        {
            if (p.Id is not null)
                _sharedProfiles![p.Id] = p;
        }
        foreach (var r in rules)
        {
            if (r.Id is not null)
                _sharedRules![r.Id] = r;
        }
        foreach (var g in infoGroups)
        {
            IndexInfoGroupRecursive(g);
        }
    }

    private void IndexRootRules(ListNode<RuleNode> rules)
    {
        foreach (var r in rules)
        {
            if (r.Id is not null)
                _sharedRules![r.Id] = r;
        }
    }

    private void IndexInfoGroupRecursive(InfoGroupNode group)
    {
        if (group.Id is not null)
            _sharedInfoGroups![group.Id] = group;
        // Also index profiles/rules within the shared group itself
        foreach (var p in group.Profiles)
        {
            if (p.Id is not null)
                _sharedProfiles![p.Id] = p;
        }
        foreach (var r in group.Rules)
        {
            if (r.Id is not null)
                _sharedRules![r.Id] = r;
        }
        foreach (var nested in group.InfoGroups)
        {
            IndexInfoGroupRecursive(nested);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Entry declaration lookup
    // ──────────────────────────────────────────────────────────────────

    private ContainerEntryBaseNode? FindEntryDeclaration(string? entryId)
    {
        if (string.IsNullOrEmpty(entryId)) return null;
        EnsureEntryDeclarations();
        return _entryDeclarations!.GetValueOrDefault(entryId);
    }

    private void EnsureEntryDeclarations()
    {
        if (_entryDeclarations is not null) return;

        _entryDeclarations = new(StringComparer.Ordinal);

        foreach (var tree in _compilation.SourceTrees)
        {
            var root = tree.GetRoot();
            if (root is CatalogueBaseNode catBase)
            {
                IndexEntryDeclarations(catBase);
            }
        }
    }

    private void IndexEntryDeclarations(CatalogueBaseNode catBase)
    {
        // Root selection entries
        foreach (var e in catBase.SelectionEntries)
            IndexEntryRecursive(e);
        foreach (var l in catBase.EntryLinks)
            IndexLinkAndTarget(l);

        // Force entries (they also inherit from ContainerEntryBase and can have profiles/rules)
        foreach (var fe in catBase.ForceEntries)
            IndexForceEntryRecursive(fe);

        // Shared entries
        foreach (var e in catBase.SharedSelectionEntries)
            IndexEntryRecursive(e);
        foreach (var g in catBase.SharedSelectionEntryGroups)
            IndexGroupRecursive(g);
    }

    private void IndexEntryRecursive(SelectionEntryNode entry)
    {
        if (entry.Id is not null)
            _entryDeclarations![entry.Id] = entry;

        foreach (var child in entry.SelectionEntries)
            IndexEntryRecursive(child);
        foreach (var child in entry.SelectionEntryGroups)
            IndexGroupRecursive(child);
        foreach (var child in entry.EntryLinks)
            IndexLinkAndTarget(child);
    }

    private void IndexGroupRecursive(SelectionEntryGroupNode group)
    {
        if (group.Id is not null)
            _entryDeclarations![group.Id] = group;

        foreach (var child in group.SelectionEntries)
            IndexEntryRecursive(child);
        foreach (var child in group.SelectionEntryGroups)
            IndexGroupRecursive(child);
        foreach (var child in group.EntryLinks)
            IndexLinkAndTarget(child);
    }

    private void IndexLinkAndTarget(EntryLinkNode link)
    {
        if (link.Id is not null)
            _entryDeclarations![link.Id] = link;
    }

    private void IndexForceEntryRecursive(ForceEntryNode fe)
    {
        if (fe.Id is not null)
            _entryDeclarations![fe.Id] = fe;

        foreach (var child in fe.ForceEntries)
            IndexForceEntryRecursive(child);
    }

    private ContainerEntryBaseNode? FindForceEntryDeclaration(string? entryId)
    {
        // Force entry declarations are indexed in the same dictionary
        return FindEntryDeclaration(entryId);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Publication resolution
    // ──────────────────────────────────────────────────────────────────

    private string? ResolvePublicationName(string? publicationId)
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
}
