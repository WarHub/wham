namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("category")]
    public class Category : RosterElementBase
    {
        [XmlArray("selections", Order = 2)]
        public Selection[] Selections { get; set; }
    }
}