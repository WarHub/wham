using BattleScribeSpec;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Maps compilation <see cref="Diagnostic"/> objects to
/// <see cref="ValidationErrorState"/> for the BattleScribeSpec test kit.
/// </summary>
internal static class DiagnosticMapper
{
    /// <summary>
    /// Filters compilation diagnostics for validation warnings and maps them
    /// to <see cref="ValidationErrorState"/> instances.
    /// </summary>
    public static IReadOnlyList<ValidationErrorState> MapValidationDiagnostics(
        IEnumerable<Diagnostic> diagnostics)
    {
        var results = new List<ValidationErrorState>();
        foreach (var diag in diagnostics)
        {
            if (!IsValidationDiagnostic(diag))
                continue;

            results.Add(MapDiagnostic(diag));
        }
        return results;
    }

    private static bool IsValidationDiagnostic(Diagnostic diag)
    {
        // Validation diagnostics use WHAM error codes in the 100-199 range (WRN_ severity)
        return diag is ValidationDiagnostic;
    }

    private static ValidationErrorState MapDiagnostic(Diagnostic diag)
    {
        if (diag is ValidationDiagnostic vd)
        {
            return new ValidationErrorState(
                Message: diag.GetMessage(),
                OwnerType: vd.OwnerType,
                OwnerId: vd.OwnerId,
                OwnerEntryId: vd.OwnerEntryId,
                EntryId: vd.EntryId,
                ConstraintId: vd.ConstraintId);
        }

        // Fallback for non-ValidationDiagnostic WRN_ diagnostics
        return new ValidationErrorState(Message: diag.GetMessage());
    }
}
