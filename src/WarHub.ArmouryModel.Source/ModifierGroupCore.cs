using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("modifierGroup")]
    public sealed partial class ModifierGroupCore : ModifierBaseCore
    {
        [XmlArray("modifiers")]
        public ImmutableArray<ModifierCore> Modifiers { get; }

        [XmlArray("modifierGroups")]
        public ImmutableArray<ModifierGroupCore> ModifierGroups { get; }
    }
}
