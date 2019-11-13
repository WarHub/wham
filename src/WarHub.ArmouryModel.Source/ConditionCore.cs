using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("condition")]
    public sealed partial class ConditionCore : SelectorBaseCore
    {
        [XmlAttribute("childId")]
        public string ChildId { get; }

        [XmlAttribute("type")]
        public ConditionKind Type { get; }
    }
}
