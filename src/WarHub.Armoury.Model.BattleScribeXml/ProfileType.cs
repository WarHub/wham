// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("profileType")]
    public sealed class ProfileType : IdentifiedGuidControllableBase,
        IIdentified, INamed, IGuidControllable
    {
        public ProfileType()
        {
            Characteristics = new List<CharacteristicType>(0);
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlArray("characteristics")]
        public List<CharacteristicType> Characteristics { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            controller.Process(Characteristics);
        }
    }
}
