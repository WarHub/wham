using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("repeat")]
    public partial class RepeatCore : SelectorBaseCore
    {
        [XmlAttribute("childId")]
        public string ChildId { get; }

        [XmlAttribute("repeats")]
        public int Repeats { get; }
    }
}
