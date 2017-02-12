namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("forceEntry")]
    public class ForceEntry : EntryBase
    {
        [XmlArray("constraints", Order = 0)]
        public Constraint[] Constraints { get; set; }

        [XmlArray("categoryEntries", Order = 1)]
        public CategoryEntry[] CategoryEntries { get; set; }

        [XmlArray("forceEntries", Order = 2)]
        public ForceEntry[] ForceEntries { get; set; }
    }
}