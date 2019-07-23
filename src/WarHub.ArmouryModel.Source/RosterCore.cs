using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot("roster", Namespace = XmlInformation.Namespaces.RosterXmlns, IsNullable = false)]
    public partial class RosterCore
    {
        [XmlAttribute("id")]
        public string Id { get; }

        [XmlAttribute("name")]
        public string Name { get; }

        [XmlAttribute("battleScribeVersion")]
        public string BattleScribeVersion { get; }

        [XmlAttribute("gameSystemId")]
        public string GameSystemId { get; }

        [XmlAttribute("gameSystemName")]
        public string GameSystemName { get; }

        [XmlAttribute("gameSystemRevision")]
        public int GameSystemRevision { get; }

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; }

        [XmlArray("costLimits")]
        public ImmutableArray<CostLimitCore> CostLimits { get; }

        [XmlArray("forces")]
        public ImmutableArray<ForceCore> Forces { get; }
    }
}
