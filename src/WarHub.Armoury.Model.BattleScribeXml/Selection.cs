// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("selection")]
    public sealed class Selection : IdentifiedGuidControllableBase,
        IIdentified, INamed, IBookIndexed, IGuidControllable
    {
        private List<Guid> _entryGuids;
        private List<Guid> _entryGroupGuids;

        public Selection()
        {
            Rules = new List<RuleMock>(0);
            Profiles = new List<ProfileMock>(0);
            Selections = new List<Selection>(0);
            EntryGuids = new List<Guid>(2);
            EntryGroupGuids = new List<Guid>(0);
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        /// <summary>
        ///     If the entry was linked to, it has following pattern: (linkId::)*(entryId).
        /// </summary>
        [XmlAttribute("entryId")]
        public string EntryId { get; set; }

        [XmlIgnore]
        public List<Guid> EntryGuids
        {
            get { return _entryGuids; }
            set { TrySetAndRaise(ref _entryGuids, value, newId => EntryId = newId); }
        }

        /// <summary>
        ///     If the entry was linked to, it has following pattern: (linkId::)*(entryId).
        /// </summary>
        [XmlAttribute("entryGroupId")]
        public string EntryGroupId { get; set; }

        [XmlIgnore]
        public List<Guid> EntryGroupGuids
        {
            get { return _entryGroupGuids; }
            set { TrySetAndRaise(ref _entryGroupGuids, value, newId => EntryGroupId = newId); }
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("points")]
        public decimal Points { get; set; }

        [XmlAttribute("number")]
        public uint Number { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlAttribute("page")]
        public string Page { get; set; }

        [XmlArray("rules")]
        public List<RuleMock> Rules { get; set; }

        [XmlArray("profiles")]
        public List<ProfileMock> Profiles { get; set; }

        [XmlArray("selections")]
        public List<Selection> Selections { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            EntryGuids = controller.ParseLinkedId(EntryId);
            EntryGroupGuids = controller.ParseLinkedId(EntryGroupId);
            controller.Process(Profiles);
            controller.Process(Rules);
            controller.Process(Selections);
        }
    }
}
