using System.Collections.Immutable;
using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(RootElementNames.Roster, Namespace = Namespaces.RosterXmlns, IsNullable = false)]
    public sealed partial record RosterCore
    {
        [XmlAttribute("id")]
        public string? Id { get; init; }

        [XmlAttribute("name")]
        public string? Name { get; init; }

        [XmlAttribute("battleScribeVersion")]
        public string? BattleScribeVersion { get; init; }

        [XmlAttribute("gameSystemId")]
        public string? GameSystemId { get; init; }

        [XmlAttribute("gameSystemName")]
        public string? GameSystemName { get; init; }

        [XmlAttribute("gameSystemRevision")]
        public int GameSystemRevision { get; init; }

        [XmlArray("costs")]
        public ImmutableArray<CostCore> Costs { get; init; } = ImmutableArray<CostCore>.Empty;

        [XmlArray("costLimits")]
        public ImmutableArray<CostLimitCore> CostLimits { get; init; } = ImmutableArray<CostLimitCore>.Empty;

        [XmlArray("forces")]
        public ImmutableArray<ForceCore> Forces { get; init; } = ImmutableArray<ForceCore>.Empty;

        [XmlElement("customNotes")]
        public string? CustomNotes { get; init; }

        [XmlArray("tags")]
        public ImmutableArray<RosterTagCore> Tags { get; init; } = ImmutableArray<RosterTagCore>.Empty;
    }
}
