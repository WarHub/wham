using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

/// <summary>
/// Composite key for <see cref="EffectiveEntryCache"/> lookups.
/// Uniquely identifies an effective entry by the declared entry,
/// optional selection context, and optional force context.
/// </summary>
internal readonly record struct EffectiveEntryKey(
    ISelectionEntryContainerSymbol Entry,
    SelectionNode? Selection,
    ForceNode? Force);
