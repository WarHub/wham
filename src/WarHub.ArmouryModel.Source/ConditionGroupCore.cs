using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("conditionGroup")]
    public sealed partial record ConditionGroupCore : CommentableCore
    {
        [XmlAttribute("type")]
        public ConditionGroupKind Type { get; init; }

        [XmlArray("conditions")]
        public ImmutableArray<ConditionCore> Conditions { get; init; } = ImmutableArray<ConditionCore>.Empty;

        [XmlArray("conditionGroups")]
        public ImmutableArray<ConditionGroupCore> ConditionGroups { get; init; } = ImmutableArray<ConditionGroupCore>.Empty;
    }
}
