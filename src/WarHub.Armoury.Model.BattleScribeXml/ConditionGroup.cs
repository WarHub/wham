namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("conditionGroup")]
    public class ConditionGroup
    {
        [XmlArray("conditions", Order = 0)]
        public List<Condition> Conditions { get; } = new List<Condition>(0);

        [XmlArray("conditionGroups", Order = 1)]
        public List<ConditionGroup> ConditionGroups { get; } = new List<ConditionGroup>(0);

        [XmlAttribute("type")]
        public ConditionGroupKind Type { get; set; }
    }
}