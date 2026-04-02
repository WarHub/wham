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

    // Shared item lookups for InfoLink resolution (built lazily)
    private Dictionary<string, ProfileNode>? _sharedProfiles;
    private Dictionary<string, RuleNode>? _sharedRules;
    private Dictionary<string, InfoGroupNode>? _sharedInfoGroups;

    // Entry declaration lookup (entryId → ContainerEntryBaseNode)
    private Dictionary<string, ContainerEntryBaseNode>? _entryDeclarations;

    public StateMapper(Compilation compilation, IReadOnlyList<ICatalogueSymbol> forceCatalogues)
    {
        _compilation = (WhamCompilation)compilation;
        _resolver = new EntryResolver();
        _forceCatalogues = forceCatalogues;
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

        var costs = MapRosterCosts(roster);
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
            selections.Add(MapSelection(selNode));
        }

        var availableEntries = _resolver.GetAvailableEntries(catalogue);
        var profiles = MapNodeProfiles(forceNode.Profiles);
        var rules = MapNodeRules(forceNode.Rules);

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

    private SelectionState MapSelection(SelectionNode selNode)
    {
        var children = new List<SelectionState>();
        foreach (var childNode in selNode.Selections)
        {
            children.Add(MapSelection(childNode));
        }

        var costs = MapSelectionCosts(selNode);
        var categories = MapSelectionCategories(selNode);

        // Resolve profiles and rules from the entry declaration
        var entryDecl = FindEntryDeclaration(selNode.EntryId);
        var profiles = ResolveSelectionProfiles(entryDecl);
        var rules = ResolveSelectionRules(entryDecl);

        var type = selNode.Type switch
        {
            SelectionEntryKind.Unit => "unit",
            SelectionEntryKind.Model => "model",
            SelectionEntryKind.Upgrade => "upgrade",
            _ => "upgrade",
        };

        var publicationName = ResolvePublicationName(selNode.PublicationId);

        return new SelectionState(
            Name: selNode.Name ?? "",
            EntryId: selNode.EntryId,
            Type: type,
            Number: selNode.Number,
            Hidden: false, // TODO Phase 4: modifier evaluation
            Costs: costs,
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

    private List<ProfileState> ResolveSelectionProfiles(ContainerEntryBaseNode? entryDecl)
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

    private List<RuleState> ResolveSelectionRules(ContainerEntryBaseNode? entryDecl)
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

        // InfoLink overrides: hidden, publicationId, page take precedence when set
        var hidden = link.Hidden || (group?.Hidden ?? target.Hidden);
        var pubId = !string.IsNullOrEmpty(link.PublicationId) ? link.PublicationId : target.PublicationId;
        var page = !string.IsNullOrEmpty(link.Page) ? link.Page : target.Page;

        return new ProfileState(
            Name: link.Name ?? target.Name ?? "",
            TypeId: target.TypeId,
            TypeName: target.TypeName,
            Hidden: hidden,
            Characteristics: chars,
            Page: page,
            PublicationId: pubId);
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
        var hidden = link.Hidden || (group?.Hidden ?? target.Hidden);
        var pubId = !string.IsNullOrEmpty(link.PublicationId) ? link.PublicationId : target.PublicationId;
        var page = !string.IsNullOrEmpty(link.Page) ? link.Page : target.Page;

        return new RuleState(
            Name: link.Name ?? target.Name ?? "",
            Description: target.Description ?? "",
            Hidden: hidden,
            Page: page,
            PublicationId: pubId);
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

    private static List<CostState> MapRosterCosts(RosterNode roster)
    {
        var result = new List<CostState>();
        foreach (var cost in roster.Costs)
        {
            result.Add(new CostState(
                Name: cost.Name ?? "",
                TypeId: cost.TypeId ?? "",
                Value: (double)cost.Value));
        }
        return result;
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
            }
            else if (root is CatalogueNode cat)
            {
                IndexSharedItems(cat.SharedProfiles, cat.SharedRules, cat.SharedInfoGroups);
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
        // Root selection entries (CatalogueBaseNode has SelectionEntries and EntryLinks, but not SelectionEntryGroups)
        foreach (var e in catBase.SelectionEntries)
            IndexEntryRecursive(e);
        foreach (var l in catBase.EntryLinks)
            IndexLinkAndTarget(l);

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
