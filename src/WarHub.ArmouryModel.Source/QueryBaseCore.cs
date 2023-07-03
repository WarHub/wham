using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial record QueryBaseCore : CommentableCore
    {
        /// <summary>
        /// Defines what is actually being counted in the query.
        /// </summary>
        [XmlAttribute("field")]
        public abstract string? Field { get; init; }

        /// <summary>
        /// Defines a scope in which the query runs.
        /// </summary>
        [XmlAttribute("scope")]
        public abstract string? Scope { get; init; }

        /// <summary>
        /// The value that the query result is compared to.
        /// </summary>
        [XmlAttribute("value")]
        public abstract decimal Value { get; init; }

        /// <summary>
        /// If <see langword="true" /> query result is calculated as a percentage
        /// of non-percentage query result to a result of query widened to "global"
        /// scope that doesn't select only values of the selector's owner; otherwise
        /// the normal query result is used.
        /// </summary>
        [XmlAttribute("percentValue")]
        public abstract bool IsValuePercentage { get; init; }

        /// <summary>
        /// If <see langword="true" /> the query calculates all selections of the parent
        /// entry; otherwise the query runs only for this specific entry selection.
        /// </summary>
        [XmlAttribute("shared")]
#pragma warning disable CA1716 // Rename virtual/interface member QueryBaseCore.Shared so that it no longer conflicts with the reserved language keyword 'Shared'. Using a reserved keyword as the name of a virtual/interface member makes it harder for consumers in other languages to override/implement the member.
        public abstract bool Shared { get; init; }
#pragma warning restore CA1716

        /// <summary>
        /// If <see langword="true" /> the scope includes all descendant selections;
        /// otherwise the it only includes the scope level.
        /// </summary>
        [XmlAttribute("includeChildSelections")]
        public abstract bool IncludeChildSelections { get; init; }

        /// <summary>
        /// If <see langword="true" /> the scope includes all descendant forces;
        /// otherwise only the one where the query originates from.
        /// </summary>
        [XmlAttribute("includeChildForces")]
        public abstract bool IncludeChildForces { get; init; }
    }
}
