namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("forceEntry")]
    public class ForceEntry : EntryBase
    {
        [XmlArray("constraints", Order = 0)]
        public List<Constraint> Constraints { get; } = new List<Constraint>(0);

        [XmlArray("categoryEntries", Order = 1)]
        public List<CategoryEntry> CategoryEntries { get; } = new List<CategoryEntry>(0);

        [XmlArray("forceEntries", Order = 2)]
        public List<ForceEntry> ForceEntries { get; } = new List<ForceEntry>(0);
    }
}