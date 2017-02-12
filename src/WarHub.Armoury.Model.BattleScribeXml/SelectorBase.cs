namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlInclude(typeof(Condition))]
    [XmlInclude(typeof(Repeat))]
    [XmlInclude(typeof(Constraint))]
    public class SelectorBase
    {
        [XmlAttribute("field")]
        public string Field { get; set; }

        [XmlAttribute("scope")]
        public string Scope { get; set; }

        [XmlAttribute("value")]
        public decimal Value { get; set; }

        [XmlAttribute("percentValue")]
        public bool PercentValue { get; set; }

        [XmlAttribute("shared")]
        public bool Shared { get; set; }

        [XmlAttribute("includeChildSelections")]
        public bool IncludeChildSelections { get; set; }

        [XmlAttribute("includeChildForces")]
        public bool IncludeChildForces { get; set; }
    }
}