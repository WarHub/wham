// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("characteristic")]
    public sealed class Characteristic : IdentifiedGuidControllableBase, ICharacteristic
    {
        public Characteristic()
        {
        }

        public Characteristic(ICharacteristic other)
        {
            Value = other.Value;
            Id = other.Id;
            Name = other.Name;
        }

        [XmlAttribute("characteristicId")]
        public override string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}
