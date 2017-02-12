namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("repeat")]
    public class Repeat : SelectorBase
    {
        [XmlAttribute("childId")]
        public string ChildId { get; set; }

        [XmlAttribute("repeats")]
        public uint Repeats { get; set; }
    }
}