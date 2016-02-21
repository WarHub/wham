// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("profile")]
    public sealed class ProfileMock : GuidControllableBase, IRosterMock
    {
        private List<Guid> _guids;

        public ProfileMock()
        {
            Characteristics = new List<RosterCharacteristic>(0);
            Guids = new List<Guid>(2);
        }

        [XmlAttribute("profileId")]
        public string Id { get; set; }

        [XmlIgnore]
        public List<Guid> Guids
        {
            get { return _guids; }
            set { TrySetAndRaise(ref _guids, value, newId => Id = newId); }
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string ProfileTypeName { get; set; }

        [XmlAttribute("hidden")]
        public bool Hidden { get; set; }

        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlAttribute("page")]
        public string Page { get; set; }

        /* content nodes */

        [XmlArray("characteristics")]
        public List<RosterCharacteristic> Characteristics { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            Guids = controller.ParseLinkedId(Id);
            controller.Process(Characteristics);
        }
    }
}
