using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("profile")]
    public partial class ProfileCore : EntryBaseCore
    {
        [XmlAttribute("profileTypeId")]
        public string ProfileTypeId { get; }

        [XmlAttribute("profileTypeName")]
        public string ProfileTypeName { get; }

        [XmlArray("characteristics")]
        public ImmutableArray<CharacteristicCore> Characteristics { get; }
    }
}
