namespace WarHub.ArmouryModel;

/// <summary>
/// Roster cost with value and limit.
/// BS Roster/Cost+CostLimit.
/// WHAM <see cref="Source.CostNode" />
/// and <see cref="Source.CostLimitNode" />.
/// </summary>
public interface IRosterCostSymbol : ISymbol
{
    /// <summary>
    /// The raw cost type identifier string from the source data.
    /// Non-lazy alternative to <see cref="CostType"/>.<see cref="ISymbol.Id"/>.
    /// </summary>
    string? TypeId { get; }

    decimal Value { get; }
    decimal? Limit { get; }
    IResourceDefinitionSymbol CostType { get; }
}
