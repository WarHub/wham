using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class ModifierBaseCore : CommentableCore
    {
        [XmlArray("repeats")]
        public ImmutableArray<RepeatCore> Repeats { get; }

        [XmlArray("conditions")]
        public ImmutableArray<ConditionCore> Conditions { get; }

        [XmlArray("conditionGroups")]
        public ImmutableArray<ConditionGroupCore> ConditionGroups { get; }
    }
}
