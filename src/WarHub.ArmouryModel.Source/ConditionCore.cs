using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("condition")]
    public sealed partial record ConditionCore : QueryFilteredBaseCore
    {
        /// <inheritdoc />
        [XmlAttribute("field")]
        public override string? Field { get; init; }

        /// <inheritdoc />
        [XmlAttribute("scope")]
        public override string? Scope { get; init; }

        /// <inheritdoc />
        [XmlAttribute("value")]
        public override decimal Value { get; init; }

        /// <inheritdoc />
        [XmlAttribute("percentValue")]
        public override bool IsValuePercentage { get; init; }

        /// <inheritdoc />
        [XmlAttribute("shared")]
        public override bool Shared { get; init; }

        /// <inheritdoc />
        [XmlAttribute("includeChildSelections")]
        public override bool IncludeChildSelections { get; init; }

        /// <inheritdoc />
        [XmlAttribute("includeChildForces")]
        public override bool IncludeChildForces { get; init; }

        /// <inheritdoc />
        [XmlAttribute("childId")]
        public override string? ChildId { get; init; }

        // TODO these should be on top?

        [XmlAttribute("type")]
        public ConditionKind Type { get; init; }

        /// <inheritdoc />
        [XmlElement("comment")]
        public override string? Comment { get; init; }
    }
}
