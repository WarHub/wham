namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("category")]
    public class Category : RosterElementBase
    {
        [XmlArray("selections", Order = 2)]
        public List<Selection> Selections { get; } = new List<Selection>(0);
    }
}