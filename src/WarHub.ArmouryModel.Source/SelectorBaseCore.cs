using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    public abstract partial class SelectorBaseCore
    {
        [XmlAttribute("field")]
        public string Field { get; }

        [XmlAttribute("scope")]
        public string Scope { get; }

        [XmlAttribute("value")]
        public decimal Value { get; }

        [XmlAttribute("percentValue")]
        public bool PercentValue { get; }

        [XmlAttribute("shared")]
        public bool Shared { get; }

        [XmlAttribute("includeChildSelections")]
        public bool IncludeChildSelections { get; }

        [XmlAttribute("includeChildForces")]
        public bool IncludeChildForces { get; }
    }
}
