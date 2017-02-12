namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("conditionGroup")]
    public class ConditionGroup
    {
        [XmlArray("conditions", Order = 0)]
        public Condition[] Conditions { get; set; }

        [XmlArray("conditionGroups", Order = 1)]
        public ConditionGroup[] ConditionGroups { get; set; }

        [XmlAttribute("type")]
        public ConditionGroupKind Type { get; set; }
    }
}