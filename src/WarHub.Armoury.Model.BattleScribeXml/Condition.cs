namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("condition")]
    public class Condition : SelectorBase
    {
        [XmlAttribute("childId")]
        public string ChildId { get; set; }

        [XmlAttribute("type")]
        public ConditionKind Type { get; set; }
    }
}