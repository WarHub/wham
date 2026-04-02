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
    private readonly ModifierEvaluator _modEval;

    // ISymbol entry lookup (entryId → ISelectionEntryContainerSymbol or IContainerEntrySymbol)
    private Dictionary<string, IEntrySymbol>? _symbolEntries;

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
        _modEval = new ModifierEvaluator(roster, compilation);
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
        var errors = new List<ValidationErrorState>(); // TODO Phase 5

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

        // Apply modifiers to get effective values
        var effectiveName = entrySym is ISelectionEntryContainerSymbol sec
            ? _modEval.GetEffectiveName(sec, selNode, force)
            : selNode.Name ?? "";
        var effectiveHidden = entrySym is not null
            ? _modEval.GetEffectiveHidden(entrySym, selNode, force)
            : false;
        var effectiveCosts = entrySym is ISelectionEntryContainerSymbol secCosts
            ? GetModifiedSelectionCosts(secCosts, selNode, force)
            : MapSelectionCosts(selNode);

        var categories = MapSelectionCategories(selNode);

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
            Page: selNode.Page,
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
            result.Add(MapProfileNode(p));
        }

        // 2. InfoLinks pointing to profiles or infogroups
        foreach (var link in entryDecl.InfoLinks)
        {
            if (link.Type == InfoLinkKind.Profile)
            {
                var target = LookupSharedProfile(link.TargetId);
                if (target is not null)
                    result.Add(MapProfileNodeWithOverrides(target, link));
            }
            else if (link.Type == InfoLinkKind.InfoGroup)
            {
                var group = LookupSharedInfoGroup(link.TargetId);
                if (group is not null)
                    CollectInfoGroupProfiles(group, result, visited: null);
            }
        }

        // 3. Inline InfoGroups
        foreach (var group in entryDecl.InfoGroups)
        {
            CollectInfoGroupProfiles(group, result, visited: null);
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
            result.Add(MapRuleNode(r));
        }

        // 2. InfoLinks pointing to rules or infogroups
        foreach (var link in entryDecl.InfoLinks)
        {
            if (link.Type == InfoLinkKind.Rule)
            {
                var target = LookupSharedRule(link.TargetId);
                if (target is not null)
                    result.Add(MapRuleNodeWithOverrides(target, link));
            }
            else if (link.Type == InfoLinkKind.InfoGroup)
            {
                var group = LookupSharedInfoGroup(link.TargetId);
                if (group is not null)
                    CollectInfoGroupRules(group, result, visited: null);
            }
        }

        // 3. Inline InfoGroups
        foreach (var group in entryDecl.InfoGroups)
        {
            CollectInfoGroupRules(group, result, visited: null);
        }

        return result;
    }

    private void CollectInfoGroupProfiles(InfoGroupNode group, List<ProfileState> result, HashSet<string>? visited)
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
            result.Add(MapProfileNode(p, group));
        }

        // InfoLinks within the group
        foreach (var link in group.InfoLinks)
        {
            if (link.Type == InfoLinkKind.Profile)
            {
                var target = LookupSharedProfile(link.TargetId);
                if (target is not null)
                    result.Add(MapProfileNodeWithOverrides(target, link, group));
            }
            else if (link.Type == InfoLinkKind.InfoGroup)
            {
                var nested = LookupSharedInfoGroup(link.TargetId);
                if (nested is not null)
                    CollectInfoGroupProfiles(nested, result, visited);
            }
        }

        // Nested inline InfoGroups
        foreach (var nested in group.InfoGroups)
        {
            CollectInfoGroupProfiles(nested, result, visited);
        }
    }

    private void CollectInfoGroupRules(InfoGroupNode group, List<RuleState> result, HashSet<string>? visited)
    {
        var groupId = group.Id;
        if (groupId is not null)
        {
            visited ??= [];
            if (!visited.Add(groupId)) return;
        }

        foreach (var r in group.Rules)
        {
            result.Add(MapRuleNode(r, group));
        }

        foreach (var link in group.InfoLinks)
        {
            if (link.Type == InfoLinkKind.Rule)
            {
                var target = LookupSharedRule(link.TargetId);
                if (target is not null)
                    result.Add(MapRuleNodeWithOverrides(target, link, group));
            }
            else if (link.Type == InfoLinkKind.InfoGroup)
            {
                var nested = LookupSharedInfoGroup(link.TargetId);
                if (nested is not null)
                    CollectInfoGroupRules(nested, result, visited);
            }
        }

        foreach (var nested in group.InfoGroups)
        {
            CollectInfoGroupRules(nested, result, visited);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Profile / Rule node → Protocol state mapping
    // ──────────────────────────────────────────────────────────────────

    private ProfileState MapProfileNode(ProfileNode p, InfoGroupNode? group = null)
    {
        var chars = new List<CharacteristicState>();
        foreach (var ch in p.Characteristics)
        {
            chars.Add(new CharacteristicState(
                Name: ch.Name ?? "",
                TypeId: ch.TypeId ?? "",
                Value: ch.Value ?? ""));
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

    private ProfileState MapProfileNodeWithOverrides(ProfileNode target, InfoLinkNode link, InfoGroupNode? group = null)
    {
        var chars = new List<CharacteristicState>();
        foreach (var ch in target.Characteristics)
        {
            chars.Add(new CharacteristicState(
                Name: ch.Name ?? "",
                TypeId: ch.TypeId ?? "",
                Value: ch.Value ?? ""));
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

    private static RuleState MapRuleNode(RuleNode r, InfoGroupNode? group = null)
    {
        return new RuleState(
            Name: r.Name ?? "",
            Description: r.Description ?? "",
            Hidden: group?.Hidden ?? r.Hidden,
            Page: r.Page,
            PublicationId: r.PublicationId);
    }

    private static RuleState MapRuleNodeWithOverrides(RuleNode target, InfoLinkNode link, InfoGroupNode? group = null)
    {
        // InfoLink overrides: hidden (OR'd), name overrides target if non-empty.
        // Page and publicationId always come from the TARGET, never the InfoLink.
        var hidden = link.Hidden || (group?.Hidden ?? target.Hidden);
        var name = !string.IsNullOrEmpty(link.Name) ? link.Name : target.Name ?? "";

        return new RuleState(
            Name: name,
            Description: target.Description ?? "",
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
        ISelectionEntryContainerSymbol entrySym,
        SelectionNode selNode,
        ForceNode force)
    {
        // Get effective per-unit costs from modifier evaluator
        var effectiveCosts = _modEval.GetEffectiveCosts(entrySym, selNode, force);
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
    }

    private void IndexSymbolEntryRecursive(ISelectionEntryContainerSymbol entry)
    {
        if (entry.Id is not null)
            _symbolEntries![entry.Id] = entry;
        foreach (var child in entry.ChildSelectionEntries)
        {
            IndexSymbolEntryRecursive(child);
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
