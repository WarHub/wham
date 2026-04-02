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

    // Reference binding parts
    StartBindingReferences = 1 << 0,
    FinishBindingReferences = 1 << 1,
    ReferencesCompleted = StartBindingReferences | FinishBindingReferences,
    Members = 1 << 2,
    MembersCompleted = 1 << 3,

    // Roster validation parts (run after all members are complete)
    EvaluateModifiers = 1 << 4,
    Validate = 1 << 5,

#pragma warning disable CA1069 // The enum member has the same constant value as member
    All = (1 << 6) - 1,

    // source symbol (catalogue/gamesystem entries)
    SourceDeclaredSymbolAll = ReferencesCompleted | Members | MembersCompleted,

    // roster symbol (adds modifier evaluation and validation)
    RosterSymbolAll = SourceDeclaredSymbolAll | EvaluateModifiers | Validate,

    // global namespace
    NamespaceAll = Members | MembersCompleted,
#pragma warning restore
}
