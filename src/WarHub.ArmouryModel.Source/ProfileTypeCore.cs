using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("profileType")]
    public partial class ProfileTypeCore
    {
        [XmlAttribute("id")]
        public string Id { get; }

        [XmlAttribute("name")]
        public string Name { get; }

        [XmlArray("characteristicTypes", Order = 0)]
        public ImmutableArray<CharacteristicTypeCore> CharacteristicTypes { get; }
    }
}
