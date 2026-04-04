namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Plain data records for resolved, modifier-applied profiles and rules.
/// These are context-dependent projections (selection + force),
/// not stable compilation symbols.
/// </summary>
internal readonly record struct ResolvedProfile(
    string Name,
    string? TypeId,
    string? TypeName,
    bool Hidden,
    ImmutableArray<ResolvedCharacteristic> Characteristics,
    string? Page,
    string? PublicationId);

internal readonly record struct ResolvedCharacteristic(
    string Name,
    string? TypeId,
    string Value);

internal readonly record struct ResolvedRule(
    string Name,
    string Description,
    bool Hidden,
    string? Page,
    string? PublicationId);
