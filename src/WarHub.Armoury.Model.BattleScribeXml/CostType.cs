namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("costType")]
    public class CostType
    {
        public CostType()
        {
            DefaultCostLimit = 0.0m;
        }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("defaultCostLimit")]
        public decimal DefaultCostLimit { get; set; }
    }
}