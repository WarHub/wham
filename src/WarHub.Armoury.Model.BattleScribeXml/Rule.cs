namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("rule")]
    public class Rule : EntryBase
    {
        [XmlElement("description", Order = 0)]
        public string Description { get; set; }
    }
}