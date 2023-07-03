using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// The repeat causes a Modifier to be applied multiple times, as calculated from
    /// the following:
    /// <see cref="RepeatCount"/> * [satisfaction count], where [satisfaction count] =
    /// ([query result] / <see cref="QueryBaseCore.Value"/>)
    /// The [satisfaction count] is rounded down unless <see cref="RoundUp"/> is <see langword="true" />
    /// in which case it's rounded up.
    /// </summary>
    [WhamNodeCore]
    [XmlType("repeat")]
    public sealed partial record RepeatCore : QueryFilteredBaseCore
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

        /// <summary>
        /// Number of times the Modifier owner of this repeat should be applied
        /// per one satisfaction in [satisfaction count] - see type summary.
        /// </summary>
        [XmlAttribute("repeats")]
        public int RepeatCount { get; init; }

        /// <summary>
        /// If <see langword="true" />, the result of dividing query result by <see cref="QueryBaseCore.Value"/>
        /// is rounded up; otherwise it's rounded down.
        /// </summary>
        [XmlAttribute("roundUp")]
        public bool RoundUp { get; init; }

        /// <inheritdoc />
        [XmlElement("comment")]
        public override string? Comment { get; init; }
    }
}
