using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("modifierGroup")]
    public sealed partial class ModifierGroupCore
    {
        [XmlArray("repeats")]
        public ImmutableArray<RepeatCore> Repeats { get; }

        [XmlArray("conditions")]
        public ImmutableArray<ConditionCore> Conditions { get; }

        [XmlArray("conditionGroups")]
        public ImmutableArray<ConditionGroupCore> ConditionGroups { get; }

        [XmlArray("modifiers")]
        public ImmutableArray<ModifierCore> Modifiers { get; }
    }
}
