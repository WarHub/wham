// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("selection")]
    public partial class Selection : RosterElementBase
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
        public List<Selection> Selections { get; } = new List<Selection>(0);

        [XmlArray("costs", Order = 3)]
        public List<Cost> Costs { get; } = new List<Cost>(0);
    }
}
