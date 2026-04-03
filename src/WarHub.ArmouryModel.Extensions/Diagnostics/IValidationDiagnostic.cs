namespace WarHub.ArmouryModel;

/// <summary>
/// Provides access to constraint validation metadata on a <see cref="Source.Diagnostic"/>.
/// Implemented by diagnostics produced during constraint validation (min/max violations,
/// cost limits, force counts, category counts).
/// </summary>
/// <remarks>
/// Use <c>diag is IValidationDiagnostic vd</c> to check if a diagnostic carries
/// validation metadata. The diagnostic ID format is <c>WHAM0100</c>–<c>WHAM0199</c>
/// (derived from <c>ErrorCode.WRN_*</c> members), but consumers should use this
/// interface rather than string-based ID filtering.
/// </remarks>
public interface IValidationDiagnostic
{
    /// <summary>
    /// ID of the roster that produced this diagnostic.
    /// Enables per-roster filtering in multi-roster compilations.
    /// </summary>
    string? RosterId { get; }

    /// <summary>
    /// Type of the entity that owns the constraint violation.
    /// Values: <c>"selection"</c>, <c>"category"</c>, <c>"force"</c>, <c>"roster"</c>.
    /// </summary>
    string? OwnerType { get; }

    /// <summary>
    /// ID of the owner instance (selection/force ID in roster).
    /// Usually <c>null</c> for entry-based owners.
    /// </summary>
    string? OwnerId { get; }

    /// <summary>
    /// Entry definition ID of the owner (e.g. <c>"se-unit-a"</c>, <c>"cat-hq"</c>).
    /// </summary>
    string? OwnerEntryId { get; }

    /// <summary>
    /// Entry whose constraint was violated (e.g. <c>"se-unit-a"</c>).
    /// </summary>
    string? EntryId { get; }

    /// <summary>
    /// ID of the specific constraint that was violated (e.g. <c>"con-max-1"</c>).
    /// </summary>
    string? ConstraintId { get; }
}
