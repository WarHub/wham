namespace WarHub.ArmouryModel;

/// <summary>
/// Represents a characteristic with its effective (modifier-applied) value.
/// </summary>
/// <param name="Name">Name of the characteristic.</param>
/// <param name="TypeId">ID of the characteristic type definition.</param>
/// <param name="Value">Effective value after modifier application.</param>
public readonly record struct EffectiveCharacteristic(string Name, string TypeId, string Value);
