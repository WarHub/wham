using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record ModifierBaseCore : CommentableCore
    {
        [XmlArray("repeats")]
        public ImmutableArray<RepeatCore> Repeats { get; init; } = ImmutableArray<RepeatCore>.Empty;

        [XmlArray("conditions")]
        public ImmutableArray<ConditionCore> Conditions { get; init; } = ImmutableArray<ConditionCore>.Empty;

        [XmlArray("conditionGroups")]
        public ImmutableArray<ConditionGroupCore> ConditionGroups { get; init; } = ImmutableArray<ConditionGroupCore>.Empty;
    }
}
