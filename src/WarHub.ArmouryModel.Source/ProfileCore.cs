using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("profile")]
    public sealed partial class ProfileCore : EntryBaseCore
    {
        [XmlAttribute("typeId")]
        public string TypeId { get; }

        [XmlAttribute("typeName")]
        public string? TypeName { get; }

        [XmlArray("characteristics")]
        public ImmutableArray<CharacteristicCore> Characteristics { get; }
    }
}
