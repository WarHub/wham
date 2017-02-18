// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
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
        public List<Cost> Costs { get; } = new List<Cost>(0);

        [XmlArray("costLimits", Order = 1)]
        public List<CostLimit> CostLimits { get; } = new List<CostLimit>(0);

        [XmlArray("forces", Order = 2)]
        public List<Force> Forces { get; } = new List<Force>(0);

        public string DefaultXmlNamespace => RosterXmlNamespace;
    }
}
