using System.Collections.Immutable;
using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("modifierGroup")]
    public sealed partial record ModifierGroupCore : ModifierBaseCore
    {
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

        [XmlArray("modifiers")]
        public ImmutableArray<ModifierCore> Modifiers { get; init; } = ImmutableArray<ModifierCore>.Empty;

        [XmlArray("modifierGroups")]
        public ImmutableArray<ModifierGroupCore> ModifierGroups { get; init; } = ImmutableArray<ModifierGroupCore>.Empty;
    }
}
