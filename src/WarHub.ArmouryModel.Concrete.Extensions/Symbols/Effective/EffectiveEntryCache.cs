using System.Collections.Concurrent;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Thread-safe cache of <see cref="EffectiveEntrySymbol"/> instances keyed by
/// (entry, selection context, force context). Lazily computes effective entries
/// on first access using the provided factory delegate.
/// <para>
/// Created per-roster and stored on <see cref="RosterSymbol"/>.
/// Since compilations are immutable, cached values are stable.
/// </para>
/// </summary>
internal sealed class EffectiveEntryCache
{
    private readonly ConcurrentDictionary<EffectiveEntryKey, EffectiveEntrySymbol> _cache = new();
    private readonly Func<ISelectionEntryContainerSymbol, SelectionNode?, ForceNode?, EffectiveEntrySymbol> _factory;

    /// <summary>
    /// Creates a new cache with the given factory for computing effective entries.
    /// </summary>
    /// <param name="factory">
    /// A delegate that computes an <see cref="EffectiveEntrySymbol"/> for a given
    /// declared entry, optional selection context, and optional force context.
    /// Typically wraps <c>ModifierEvaluator</c> logic.
    /// </param>
    public EffectiveEntryCache(
        Func<ISelectionEntryContainerSymbol, SelectionNode?, ForceNode?, EffectiveEntrySymbol> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Gets or lazily computes the effective entry for the given key.
    /// </summary>
    public EffectiveEntrySymbol GetEffectiveEntry(
        ISelectionEntryContainerSymbol entry,
        SelectionNode? selection,
        ForceNode? force)
    {
        return _cache.GetOrAdd(
            new EffectiveEntryKey(entry, selection, force),
            key => _factory(key.Entry, key.Selection, key.Force));
    }
}
