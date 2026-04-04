namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Concrete implementation of <see cref="IEffectiveRuleSymbol"/>.
/// </summary>
internal sealed class EffectiveRuleSymbol : IEffectiveRuleSymbol
{
    public EffectiveRuleSymbol(
        string name,
        string description,
        bool isHidden,
        string? page,
        string? publicationId)
    {
        Name = name;
        Description = description;
        IsHidden = isHidden;
        Page = page;
        PublicationId = publicationId;
    }

    public string Name { get; }
    public string Description { get; }
    public bool IsHidden { get; }
    public string? Page { get; }
    public string? PublicationId { get; }
}
