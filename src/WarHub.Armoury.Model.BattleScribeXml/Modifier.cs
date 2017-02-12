namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("modifier")]
    public class Modifier
    {
        [XmlArray("repeats", Order = 0)]
        public Repeat[] Repeats { get; set; }

        [XmlArray("conditions", Order = 1)]
        public Condition[] Conditions { get; set; }

        [XmlArray("conditionGroups", Order = 2)]
        public ConditionGroup[] ConditionGroups { get; set; }

        [XmlAttribute("type")]
        public ModifierKind Type { get; set; }

        [XmlAttribute("field")]
        public string Field { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}