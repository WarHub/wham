using System.Collections.Immutable;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(RootElementNames.Roster, Namespace = Namespaces.RosterXmlns, IsNullable = false)]
    public sealed partial class RosterCore
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
