using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record ModifierBaseCore : CommentableCore
    {
        [XmlArray("repeats")]
        public abstract ImmutableArray<RepeatCore> Repeats { get; init; }

        [XmlArray("conditions")]
        public abstract ImmutableArray<ConditionCore> Conditions { get; init; }

        [XmlArray("conditionGroups")]
        public abstract ImmutableArray<ConditionGroupCore> ConditionGroups { get; init; }
    }
}
