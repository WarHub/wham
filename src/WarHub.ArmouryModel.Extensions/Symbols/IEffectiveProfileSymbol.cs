namespace WarHub.ArmouryModel;

/// <summary>
/// A profile with effective (modifier-applied) values, fully resolved from
/// the entry's resource graph. Profiles may come from direct definitions,
/// info links, or info groups.
/// </summary>
public interface IEffectiveProfileSymbol
{
    /// <summary>
    /// Effective name — overridden by info link name if non-empty, otherwise the target profile name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Hidden flag from the traversal path (group or link hidden OR'd with profile hidden).
    /// </summary>
    bool IsHidden { get; }

    /// <summary>
    /// Profile type definition ID (e.g. "Unit", "Weapon").
    /// </summary>
    string? TypeId { get; }

    /// <summary>
    /// Profile type definition name.
    /// </summary>
    string? TypeName { get; }

    /// <summary>
    /// Page from the profile's publication reference.
    /// </summary>
    string? Page { get; }

    /// <summary>
    /// Publication ID from the profile's publication reference.
    /// </summary>
    string? PublicationId { get; }

    /// <summary>
    /// Characteristics with modifier-applied values.
    /// </summary>
    ImmutableArray<EffectiveCharacteristic> Characteristics { get; }
}
