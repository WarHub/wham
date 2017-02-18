// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlInclude(typeof(Force))]
    [XmlInclude(typeof(Category))]
    [XmlInclude(typeof(Selection))]
    public abstract class RosterElementBase
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("entryId")]
        public string EntryId { get; set; }

        [XmlArray("rules", Order = 0)]
        public Rule[] Rules { get; set; }

        [XmlArray("profiles", Order = 1)]
        public Profile[] Profiles { get; set; }
    }
}
