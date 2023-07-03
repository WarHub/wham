using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("modifier")]
    public sealed partial record ModifierCore : ModifierBaseCore
    {
        [XmlAttribute("type")]
        public ModifierKind Type { get; init; }

        [XmlAttribute("field")]
        public string? Field { get; init; }

        [XmlAttribute("value")]
        public string? Value { get; init; }

        /// <inheritdoc />
        [XmlElement("comment")]
        public override string? Comment { get; init; }

        /// <inheritdoc />
        [XmlArray("repeats")]
        public override ImmutableArray<RepeatCore> Repeats { get; init; } = ImmutableArray<RepeatCore>.Empty;

        /// <inheritdoc />
        [XmlArray("conditions")]
        public override ImmutableArray<ConditionCore> Conditions { get; init; } = ImmutableArray<ConditionCore>.Empty;

        /// <inheritdoc />
        [XmlArray("conditionGroups")]
        public override ImmutableArray<ConditionGroupCore> ConditionGroups { get; init; } = ImmutableArray<ConditionGroupCore>.Empty;
    }
}
