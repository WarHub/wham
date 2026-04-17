using WarHub.ArmouryModel.Source;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.EditorServices;

public static class RosterOperations
{
    public static IRosterOperation Identity => IdentityRosterOperation.Instance;

    public static CreateRosterOperation CreateRoster() => new();

    public static ChangeCostLimitOperation ChangeCostLimit(CostLimitNode costLimit, decimal newValue) =>
        new(costLimit.TypeId!, newValue);

    public static ChangeCostLimitOperation ChangeCostLimit(string typeId, decimal newValue) =>
        new(typeId, newValue);

    public static ChangeRosterNameOperation ChangeRosterName(string name) =>
        new(name);

    public static AddForceOperation AddForce(ForceEntryNode forceEntry) => new(forceEntry);

    public static AddSelectionOperation AddSelection(SelectionEntryNode selectionEntry, ForceNode force) =>
        new(selectionEntry, force);

    public static AddSelectionFromLinkOp AddSelectionFromLink(SelectionEntryNode selectionEntry, EntryLinkNode link, string force) =>
        new(selectionEntry, link, force);

    public static RemoveForceOperation RemoveForce(ForceNode force) => new(force);

    public static RemoveSelectionOperation RemoveSelection(SelectionNode selection) =>
        new(selection);

    public static ChangeSelectionCountOperation ChangeCountOf(SelectionNode selection, int newCount) =>
        new(selection, newCount);
}

public sealed class IdentityRosterOperation : IRosterOperation
{
    private IdentityRosterOperation() { }

    public static IdentityRosterOperation Instance { get; } = new();

    public RosterOperationKind Kind => RosterOperationKind.Identity;

    public RosterState Apply(RosterState baseState) => baseState;
}

public abstract record RosterOperationBase : IRosterOperation
{
    RosterOperationKind IRosterOperation.Kind => Kind;

    protected virtual RosterOperationKind Kind => RosterOperationKind.Unknown;

    protected virtual RosterNode TransformRoster(RosterState state) => state.RosterRequired;

    RosterState IRosterOperation.Apply(RosterState baseState)
    {
        return baseState.ReplaceRoster(TransformRoster(baseState).WithUpdatedCostTotals());
    }
}

public record CreateRosterOperation : IRosterOperation
{
    public string? Name { get; init; }

    RosterState IRosterOperation.Apply(RosterState baseState)
    {
        var gamesystem = baseState.Gamesystem;
        var roster =
            Roster(gamesystem, Name)
            .WithCostLimits(gamesystem.CostTypes
                .Select(costType => CostLimit(costType, costType.DefaultCostLimit))
                .ToArray())
            .WithCosts(gamesystem.CostTypes
                .Select(costType => Cost(costType, 0m))
                .ToArray());
        var rosterTree = SourceTree.CreateForRoot(roster);
        return new RosterState(baseState.Compilation.AddRosterTrees(rosterTree));
    }
}

public record ChangeCostLimitOperation(string TypeId, decimal NewValue) : RosterOperationBase
{
    protected override RosterOperationKind Kind => RosterOperationKind.ModifyCostLimits;

    protected override RosterNode TransformRoster(RosterState state)
    {
        var roster = state.RosterRequired;

        if (roster.CostLimits.FirstOrDefault(x => x.TypeId == TypeId) is { } costLimit)
        {
            return roster.Replace(costLimit, x => x.WithValue(NewValue));
        }
        var costType = state.Gamesystem.CostTypes.FirstOrDefault(type => type.Id == TypeId)
            ?? throw new InvalidOperationException($"Cost type '{TypeId}' not found in game system.");
        return roster.AddCostLimits(CostLimit(costType));
    }
}

public record ChangeRosterNameOperation(string Name) : RosterOperationBase
{
    protected override RosterOperationKind Kind => RosterOperationKind.RenameRoster;

    protected override RosterNode TransformRoster(RosterState state)
    {
        var roster = state.RosterRequired;
        return roster.WithName(Name);
    }
}

public record AddForceOperation(ForceEntryNode ForceEntry) : RosterOperationBase
{
    protected override RosterOperationKind Kind => RosterOperationKind.AddForce;

    public Func<RosterNode, ForceNode>? SelectForceParent { get; init; }

    protected override RosterNode TransformRoster(RosterState state)
    {
        var roster = state.RosterRequired;
        // TODO add categories, rules, profiles, catalogue cost types to cost limits
        var force = Force(ForceEntry);
        return SelectForceParent is null
            ? roster.AddForces(force)
            : roster.ReplaceFluent(SelectForceParent, x => x.AddForces(force));
    }
}

public record RemoveForceOperation(ForceNode Force) : RosterOperationBase
{
    protected override RosterOperationKind Kind => RosterOperationKind.RemoveForce;

    protected override RosterNode TransformRoster(RosterState state)
    {
        return state.RosterRequired.Remove(Force);
    }
}

public record AddSelectionOperation(SelectionEntryNode SelectionEntry, ForceNode Force) : RosterOperationBase
{
    protected override RosterOperationKind Kind => RosterOperationKind.AddSelection;

    protected override RosterNode TransformRoster(RosterState state)
    {
        var roster = state.RosterRequired;
        var selectionEntryId = SelectionEntry.Id;
        if (selectionEntryId is null)
            return roster; // TODO add diagnostic invalid data
        var selection =
            Selection(SelectionEntry, selectionEntryId)
            .AddCosts(SelectionEntry.Costs);
        // TODO add selection categories, rules, profiles
        // TODO add subselections
        return roster.Replace(Force, x => x.AddSelections(selection));
    }
}

public record AddSelectionFromLinkOp(SelectionEntryNode SelectionEntry, EntryLinkNode EntryLink, string ForceId) : RosterOperationBase
{
    protected override RosterOperationKind Kind => RosterOperationKind.AddSelection;

    protected override RosterNode TransformRoster(RosterState state)
    {
        var roster = state.RosterRequired;
        var selectionEntryId = SelectionEntry.Id;
        if (selectionEntryId is null)
            return roster; // TODO add diagnostic invalid data
        var selection =
            Selection(SelectionEntry, selectionEntryId)
            .AddCosts(EntryLink.Costs);
        // .WithCategories(CategoryList( catNodes));
        // TODO add selection categories, rules, profiles
        // TODO add subselections

        // Use ID because the ForceNode object becomes invalid after other operations,
        // such as a prior selectionAdd
        var force = roster.Forces.FirstOrDefault(f => f.Id == ForceId);

        if (force != null)
            return roster.Replace(force, x => x.AddSelections(selection));
        else
            return roster;
    }
}

/// <summary>
/// Adds a root selection to a force by resolving the entry and force via <see cref="SymbolKey"/>.
/// </summary>
public record AddRootEntryFromSymbol(SymbolKey EntryKey, SymbolKey ForceKey, int Count = 1) : RosterOperationBase
{
    protected override RosterOperationKind Kind => RosterOperationKind.AddSelection;

    protected override RosterNode TransformRoster(RosterState state)
    {
        var roster = state.RosterRequired;
        var resolution = EntryKey.Resolve(state.Compilation);
        if (resolution.Kind != SymbolKeyResolutionKind.Resolved || resolution.Symbol is not ISelectionEntryContainerSymbol entryLocal)
        {
            throw new InvalidOperationException(
                $"Entry key ({EntryKey.Kind}, '{EntryKey.SymbolId}') could not be resolved: {resolution.Kind}.");
        }
        var forceResolution = ForceKey.Resolve(state.Compilation);
        if (forceResolution.Kind != SymbolKeyResolutionKind.Resolved || forceResolution.Symbol is not IForceSymbol forceSymbol)
        {
            throw new InvalidOperationException(
                $"Force key ({ForceKey.Kind}, '{ForceKey.SymbolId}') could not be resolved: {forceResolution.Kind}.");
        }
        var forceId = forceSymbol.Id;
        var entries = !entryLocal.IsReference
            ? new[] { entryLocal }
            : new[] { entryLocal, entryLocal.ReferencedEntry! };
        // TODO how does BS work when Link declares costs as well as the Target entry?
        var costNodes = entries
            .SelectMany(x => x.Costs)
            .Where(x => x.Value > 0 && x.Type?.Id is not null)
            .Select(x => Cost(x.Name, x.Type!.Id, x.Value))
            .ToList();
        // TODO handle primary set in both link and target entry, deduplicate categories
        var catList = entries
            .SelectMany(x => x.Categories)
            .Select(x => Category(x.ReferencedEntry!.GetEntryDeclaration()!, x.ReferencedEntry!.Id).WithPrimary(x.IsPrimaryCategory))
            .ToList();
        var selectionEntryNode = (entryLocal.IsReference ? entryLocal.ReferencedEntry! : entryLocal).GetEntryDeclaration()!;
        for (var i = 0; i < Count; i++)
        {
            // Look up by ID each iteration because the ForceNode object becomes
            // invalid after prior mutations (immutable tree replacement).
            var force = roster.Forces.FirstOrDefault(f => f.Id == forceId)
                ?? throw new InvalidOperationException($"Force '{forceId}' not found in roster.");
            var selection =
                Selection(selectionEntryNode, selectionEntryNode.Id!)
                .AddCosts(costNodes)
                .AddCategories(catList);

            roster = roster.Replace(force, x => x.AddSelections(selection));
        }
        return roster;
    }
}

public record RemoveSelectionOperation(SelectionNode Selection) : RosterOperationBase
{
    protected override RosterOperationKind Kind => RosterOperationKind.RemoveSelection;

    protected override RosterNode TransformRoster(RosterState state)
    {
        return state.RosterRequired.Remove(Selection);
    }
}

public record ChangeSelectionCountOperation(SelectionNode Selection, int NewCount) : RosterOperationBase
{
    protected override RosterOperationKind Kind => RosterOperationKind.ModifySelectionCount;

    protected override RosterNode TransformRoster(RosterState state)
    {
        var roster = state.RosterRequired;
        // TODO subselections (collective?)
        return roster.Replace(Selection, x => x.WithUpdatedNumberAndCosts(NewCount))!;
    }
}
