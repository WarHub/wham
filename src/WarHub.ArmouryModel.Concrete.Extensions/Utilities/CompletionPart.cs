namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// This enum describes the types of components that could give
/// us diagnostics.  We shouldn't read the list of diagnostics
/// until all of these types are accounted for.
/// From: https://sourceroslyn.io/#Microsoft.CodeAnalysis.CSharp/Symbols/CompletionPart.cs
/// </summary>
/// <remarks>
/// Bound reference fields are self-completing (each property binds itself on first access
/// via <c>Interlocked.CompareExchange</c>) and do not require a dedicated completion phase.
/// </remarks>
[Flags]
internal enum CompletionPart
{
    None = 0,

    // Phase 1: Members
    Members = 1 << 0,
    MembersCompleted = 1 << 1,

    // Phase 2: Effective entries (computed by RosterSymbol, auto-completed by others)
    StartEffectiveEntries = 1 << 2,
    FinishEffectiveEntries = 1 << 3,
    EffectiveEntriesCompleted = StartEffectiveEntries | FinishEffectiveEntries,

    // Phase 3: Check references (inspect self-completed bound fields for errors)
    StartCheckReferences = 1 << 4,
    FinishCheckReferences = 1 << 5,
    CheckReferencesCompleted = StartCheckReferences | FinishCheckReferences,

    // Phase 4: Check constraints (evaluated by RosterSymbol, auto-completed by others)
    StartCheckConstraints = 1 << 6,
    FinishCheckConstraints = 1 << 7,
    CheckConstraintsCompleted = StartCheckConstraints | FinishCheckConstraints,

#pragma warning disable CA1069 // The enum member has the same constant value as member
    All = (1 << 8) - 1,

    // source symbol
    SourceDeclaredSymbolAll = Members | MembersCompleted
        | EffectiveEntriesCompleted | CheckReferencesCompleted | CheckConstraintsCompleted,

    // roster symbol (same as SourceDeclaredSymbolAll for now, but explicit)
    RosterSymbolAll = SourceDeclaredSymbolAll,

    // global namespace
    NamespaceAll = Members | MembersCompleted,
#pragma warning restore
}
