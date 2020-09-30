using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("modifierGroup")]
    public sealed partial record ModifierGroupCore : ModifierBaseCore
    {
        [XmlArray("modifiers")]
        public ImmutableArray<ModifierCore> Modifiers { get; init; } = ImmutableArray<ModifierCore>.Empty;

        [XmlArray("modifierGroups")]
        public ImmutableArray<ModifierGroupCore> ModifierGroups { get; init; } = ImmutableArray<ModifierGroupCore>.Empty;
    }
}
