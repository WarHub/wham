namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// This enum describes the types of components that could give
/// us diagnostics.  We shouldn't read the list of diagnostics
/// until all of these types are accounted for.
/// From: https://sourceroslyn.io/#Microsoft.CodeAnalysis.CSharp/Symbols/CompletionPart.cs
/// </summary>
[Flags]
internal enum CompletionPart
{
    None = 0,

    // Phase 1: Binding
    StartBindingReferences = 1 << 0,
    FinishBindingReferences = 1 << 1,
    ReferencesCompleted = StartBindingReferences | FinishBindingReferences,

    // Phase 2: Members
    Members = 1 << 2,
    MembersCompleted = 1 << 3,

    // Phase 3: Effective entries (computed by RosterSymbol, auto-completed by others)
    StartEffectiveEntries = 1 << 4,
    FinishEffectiveEntries = 1 << 5,
    EffectiveEntriesCompleted = StartEffectiveEntries | FinishEffectiveEntries,

    // Phase 4: Constraints (evaluated by RosterSymbol, auto-completed by others)
    StartConstraints = 1 << 6,
    FinishConstraints = 1 << 7,
    ConstraintsCompleted = StartConstraints | FinishConstraints,

#pragma warning disable CA1069 // The enum member has the same constant value as member
    All = (1 << 8) - 1,

    // source symbol
    SourceDeclaredSymbolAll = ReferencesCompleted | Members | MembersCompleted
        | EffectiveEntriesCompleted | ConstraintsCompleted,

    // roster symbol (same as SourceDeclaredSymbolAll for now, but explicit)
    RosterSymbolAll = SourceDeclaredSymbolAll,

    // global namespace
    NamespaceAll = Members | MembersCompleted,
#pragma warning restore
}
