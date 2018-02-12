using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("modifier")]
    public partial class ModifierCore
    {
        [XmlAttribute("type")]
        public ModifierKind Type { get; }

        [XmlAttribute("field")]
        public string Field { get; }

        [XmlAttribute("value")]
        public string Value { get; }

        [XmlArray("repeats")]
        public ImmutableArray<RepeatCore> Repeats { get; }

        [XmlArray("conditions")]
        public ImmutableArray<ConditionCore> Conditions { get; }

        [XmlArray("conditionGroups")]
        public ImmutableArray<ConditionGroupCore> ConditionGroups { get; }
    }
}
