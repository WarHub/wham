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

    public StateMapper(Compilation compilation, IReadOnlyList<ICatalogueSymbol> forceCatalogues)
    {
        _compilation = (WhamCompilation)compilation;
        _resolver = new EntryResolver();
        _forceCatalogues = forceCatalogues;
    }

    /// <summary>
    /// Maps the roster in the compilation to a <see cref="RosterState"/>.
    /// </summary>
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
        // TODO Phase 5: validation errors
        var errors = new List<ValidationErrorState>();

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

        // Map force profiles and rules
        var profiles = MapForceProfiles(forceNode);
        var rules = MapForceRules(forceNode);

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
        // TODO Phase 4: effective name/hidden after modifiers
        // TODO Phase 6: profiles, rules

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
            Categories: categories.Count > 0 ? categories : null,
            Page: selNode.Page,
            PublicationId: selNode.PublicationId,
            PublicationName: publicationName);
    }

    private List<CostState> MapSelectionCosts(SelectionNode selNode)
    {
        var costs = new List<CostState>();
        foreach (var costNode in selNode.Costs)
        {
            // Cost value * Number (unless collective — but collective handling is Phase 4)
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

    private List<CostState> MapRosterCosts(RosterNode roster)
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

    private static List<ProfileState> MapForceProfiles(ForceNode forceNode)
    {
        // TODO Phase 6: resolve profiles from force entry declaration
        var profiles = new List<ProfileState>();
        foreach (var profileNode in forceNode.Profiles)
        {
            var chars = new List<CharacteristicState>();
            foreach (var ch in profileNode.Characteristics)
            {
                chars.Add(new CharacteristicState(
                    Name: ch.Name ?? "",
                    TypeId: ch.TypeId ?? "",
                    Value: ch.Value ?? ""));
            }
            profiles.Add(new ProfileState(
                Name: profileNode.Name ?? "",
                TypeId: profileNode.TypeId,
                TypeName: profileNode.TypeName,
                Hidden: profileNode.Hidden,
                Characteristics: chars,
                Page: profileNode.Page,
                PublicationId: profileNode.PublicationId));
        }
        return profiles;
    }

    private static List<RuleState> MapForceRules(ForceNode forceNode)
    {
        // TODO Phase 6: resolve rules from force entry declaration
        var rules = new List<RuleState>();
        foreach (var ruleNode in forceNode.Rules)
        {
            rules.Add(new RuleState(
                Name: ruleNode.Name ?? "",
                Description: ruleNode.Description ?? "",
                Hidden: ruleNode.Hidden,
                Page: ruleNode.Page,
                PublicationId: ruleNode.PublicationId));
        }
        return rules;
    }

    private string? ResolvePublicationName(string? publicationId)
    {
        if (string.IsNullOrEmpty(publicationId))
            return null;

        var gsSym = _compilation.GlobalNamespace.RootCatalogue;

        // Search in gamesystem resource definitions
        foreach (var rd in gsSym.ResourceDefinitions)
        {
            if (rd is IPublicationSymbol pub && pub.Id == publicationId)
                return pub.Name;
        }

        // Search in catalogues
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
