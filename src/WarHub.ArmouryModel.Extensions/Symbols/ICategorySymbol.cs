namespace WarHub.ArmouryModel;

/// <summary>
/// Category instance.
/// BS Category.
/// WHAM <see cref="Source.CategoryNode"/>.
/// </summary>
public interface ICategorySymbol : IContainerSymbol
{
    bool IsPrimaryCategory { get; }

    new ICategoryEntrySymbol SourceEntry { get; }
}
