// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlRoot("roster", Namespace = RosterXmlNamespace, IsNullable = false)]
    public class Roster : IXmlProperties
    {
        public const string RosterXmlNamespace = "http://www.battlescribe.net/schema/rosterSchema";

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("battleScribeVersion")]
        public string BattleScribeVersion { get; set; }

        [XmlAttribute("gameSystemId")]
        public string GameSystemId { get; set; }

        [XmlAttribute("gameSystemName")]
        public string GameSystemName { get; set; }

        [XmlAttribute("gameSystemRevision")]
        public uint GameSystemRevision { get; set; }

        [XmlArray("costs", Order = 0)]
        public Cost[] Costs { get; set; }

        [XmlArray("costLimits", Order = 1)]
        public CostLimit[] CostLimits { get; set; }

        [XmlArray("forces", Order = 2)]
        public Force[] Forces { get; set; }

        public string DefaultXmlNamespace => RosterXmlNamespace;
    }
}
