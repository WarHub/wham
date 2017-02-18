namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("modifier")]
    public class Modifier
    {
        [XmlArray("repeats", Order = 0)]
        public List<Repeat> Repeats { get; } = new List<Repeat>(0);

        [XmlArray("conditions", Order = 1)]
        public List<Condition> Conditions { get; } = new List<Condition>(0);

        [XmlArray("conditionGroups", Order = 2)]
        public List<ConditionGroup> ConditionGroups { get; } = new List<ConditionGroup>(0);

        [XmlAttribute("type")]
        public ModifierKind Type { get; set; }

        [XmlAttribute("field")]
        public string Field { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}