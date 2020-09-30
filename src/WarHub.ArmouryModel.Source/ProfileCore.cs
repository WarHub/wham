using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("profile")]
    public sealed partial record ProfileCore : EntryBaseCore
    {
        [XmlAttribute("typeId")]
        public string? TypeId { get; init; }

        [XmlAttribute("typeName")]
        public string? TypeName { get; init; }

        [XmlArray("characteristics")]
        public ImmutableArray<CharacteristicCore> Characteristics { get; init; } = ImmutableArray<CharacteristicCore>.Empty;
    }
}
