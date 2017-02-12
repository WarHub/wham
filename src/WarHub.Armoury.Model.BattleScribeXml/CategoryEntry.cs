namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("categoryEntry")]
    public class CategoryEntry : EntryBase
    {
        [XmlArray("constraints", Order = 0)]
        public Constraint[] Constraints { get; set; }
    }
}