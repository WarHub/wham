namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("categoryEntry")]
    public class CategoryEntry : EntryBase
    {
        [XmlArray("constraints", Order = 0)]
        public List<Constraint> Constraints { get; } = new List<Constraint>(0);
    }
}