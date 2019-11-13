using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("repeat")]
    public sealed partial class RepeatCore : SelectorBaseCore
    {
        [XmlAttribute("childId")]
        public string ChildId { get; }

        [XmlAttribute("repeats")]
        public int Repeats { get; }

        [XmlAttribute("roundUp")]
        public bool IsRoundUp { get; }
    }
}
