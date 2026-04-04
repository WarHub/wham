namespace WarHub.ArmouryModel;

/// <summary>
/// A rule with effective (modifier-applied) values, fully resolved from
/// the entry's resource graph. Rules may come from direct definitions,
/// info links, or info groups.
/// </summary>
public interface IEffectiveRuleSymbol
{
    /// <summary>
    /// Effective name — overridden by info link name if non-empty, otherwise the target rule name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Effective description text after modifier application.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Hidden flag from the traversal path (group or link hidden OR'd with rule hidden).
    /// </summary>
    bool IsHidden { get; }

    /// <summary>
    /// Page from the rule's publication reference.
    /// </summary>
    string? Page { get; }

    /// <summary>
    /// Publication ID from the rule's publication reference.
    /// </summary>
    string? PublicationId { get; }
}
