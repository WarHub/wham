namespace WarHub.ArmouryModel;

/// <summary>
/// Roster root.
/// BS Roster.
/// WHAM <see cref="Source.RosterNode" />.
/// </summary>
public interface IRosterSymbol : IModuleSymbol, IForceContainerSymbol
{
    string? CustomNotes { get; }
    ImmutableArray<IRosterCostSymbol> Costs { get; }

    /// <summary>
    /// Returns the effective (modifier-applied) version of a declared entry.
    /// Optionally scoped to a specific selection and/or force context.
    /// When modifier evaluation is not available, returns <paramref name="declaredEntry"/> as-is.
    /// </summary>
    ISelectionEntryContainerSymbol GetEffectiveEntry(
        ISelectionEntryContainerSymbol declaredEntry,
        ISelectionSymbol? selection = null,
        IForceSymbol? force = null);
}
