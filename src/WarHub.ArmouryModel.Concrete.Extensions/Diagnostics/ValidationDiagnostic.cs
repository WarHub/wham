using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// A diagnostic with validation-specific metadata for mapping to
/// <c>BattleScribeSpec.ValidationErrorState</c> in the Spec adapter layer.
/// </summary>
internal sealed class ValidationDiagnostic : DiagnosticWithInfo, IValidationDiagnostic
{
    internal ValidationDiagnostic(
        DiagnosticInfo info,
        Location location,
        string? rosterId = null,
        string? ownerType = null,
        string? ownerId = null,
        string? ownerEntryId = null,
        string? entryId = null,
        string? constraintId = null)
        : base(info, location)
    {
        RosterId = rosterId;
        OwnerType = ownerType;
        OwnerId = ownerId;
        OwnerEntryId = ownerEntryId;
        EntryId = entryId;
        ConstraintId = constraintId;
    }

    public string? RosterId { get; }
    public string? OwnerType { get; }
    public string? OwnerId { get; }
    public string? OwnerEntryId { get; }
    public string? EntryId { get; }
    public string? ConstraintId { get; }

    public override string ToString() =>
        DiagnosticFormatter.Instance.Format(this, formatter: null);

    internal override Diagnostic WithLocation(Location location) =>
        new ValidationDiagnostic(Info, location, RosterId, OwnerType, OwnerId, OwnerEntryId, EntryId, ConstraintId);

    internal override Diagnostic WithSeverity(DiagnosticSeverity severity) =>
        new ValidationDiagnostic(Info.GetInstanceWithSeverity(severity), Location, RosterId, OwnerType, OwnerId, OwnerEntryId, EntryId, ConstraintId);

    internal override Diagnostic WithIsSuppressed(bool isSuppressed) =>
        this; // Validation diagnostics are not suppressible
}
