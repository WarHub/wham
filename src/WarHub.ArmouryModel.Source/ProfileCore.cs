using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("profile")]
    public partial class ProfileCore : EntryBaseCore
    {
        [XmlAttribute("typeId")]
        public string ProfileTypeId { get; }

        [XmlAttribute("typeName")]
        public string ProfileTypeName { get; }

        [XmlArray("modifiers")]
        public ImmutableArray<ModifierCore> Modifiers { get; }

        [XmlArray("modifierGroups")]
        public ImmutableArray<ModifierGroupCore> ModifierGroups { get; }

        [XmlArray("characteristics")]
        public ImmutableArray<CharacteristicCore> Characteristics { get; }
    }
}
