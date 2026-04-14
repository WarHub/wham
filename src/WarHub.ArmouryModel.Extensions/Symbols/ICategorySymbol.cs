namespace WarHub.ArmouryModel;

/// <summary>
/// Category instance.
/// BS Category.
/// WHAM <see cref="Source.CategoryNode"/>.
/// </summary>
public interface ICategorySymbol : IContainerEntryInstanceSymbol
{
    /// <summary>
    /// The entry ID string of the catalogue category entry this instance refers to.
    /// Read from the roster data; does not trigger lazy symbol binding.
    /// </summary>
    string? EntryId { get; }

    bool IsPrimaryCategory { get; }

    new ICategoryEntrySymbol SourceEntry { get; }
}
