namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("cost")]
    public class Cost
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("costTypeId")]
        public string CostTypeId { get; set; }

        [XmlAttribute("value")]
        public decimal Value { get; set; }
    }
}