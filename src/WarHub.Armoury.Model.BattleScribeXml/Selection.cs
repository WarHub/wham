// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("selection")]
    public class Selection : RosterElementBase
    {
        [XmlAttribute("entryGroupId")]
        public string EntryGroupId { get; set; }

        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlAttribute("page")]
        public string Page { get; set; }

        [XmlAttribute("number")]
        public int Number { get; set; }

        [XmlAttribute("type")]
        public SelectionEntryKind Type { get; set; }

        [XmlArray("selections", Order = 2)]
        public Selection[] Selections { get; set; }

        [XmlArray("costs", Order = 3)]
        public Cost[] Costs { get; set; }
    }
}
