// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("characteristic")]
    public sealed class CharacteristicType : IdentifiedGuidControllableBase,
        IIdentified, INamed, IGuidControllable
    {
        public CharacteristicType()
        {
        }

        public CharacteristicType(CharacteristicType other)
        {
            Id = other.Id;
            Name = other.Name;
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
