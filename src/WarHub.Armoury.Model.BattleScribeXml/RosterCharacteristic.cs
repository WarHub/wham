// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("characteristic")]
    public sealed class RosterCharacteristic : IdentifiedGuidControllableBase, ICharacteristic
    {
        public RosterCharacteristic()
        {
        }

        public RosterCharacteristic(ICharacteristic other)
        {
            Guid = other.Guid;
            Id = other.Id;
            Name = other.Name;
            Value = other.Value;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("characteristicId")]
        public override string Id { get; set; }
    }
}
