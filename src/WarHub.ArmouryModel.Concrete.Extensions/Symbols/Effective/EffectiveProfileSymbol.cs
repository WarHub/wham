namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Concrete implementation of <see cref="IEffectiveProfileSymbol"/>.
/// </summary>
internal sealed class EffectiveProfileSymbol : IEffectiveProfileSymbol
{
    public EffectiveProfileSymbol(
        string name,
        bool isHidden,
        string? typeId,
        string? typeName,
        string? page,
        string? publicationId,
        ImmutableArray<EffectiveCharacteristic> characteristics)
    {
        Name = name;
        IsHidden = isHidden;
        TypeId = typeId;
        TypeName = typeName;
        Page = page;
        PublicationId = publicationId;
        Characteristics = characteristics;
    }

    public string Name { get; }
    public bool IsHidden { get; }
    public string? TypeId { get; }
    public string? TypeName { get; }
    public string? Page { get; }
    public string? PublicationId { get; }
    public ImmutableArray<EffectiveCharacteristic> Characteristics { get; }
}
