using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("conditionGroup")]
    public partial class ConditionGroupCore
    {
        [XmlAttribute("type")]
        public ConditionGroupKind Type { get; }

        [XmlArray("conditions", Order = 0)]
        public ImmutableArray<ConditionCore> Conditions { get; }

        [XmlArray("conditionGroups", Order = 1)]
        public ImmutableArray<ConditionGroupCore> ConditionGroups { get; }
    }
}
