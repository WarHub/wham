using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("condition")]
    public sealed partial record ConditionCore : QueryFilteredBaseCore
    {
        [XmlAttribute("type")]
        public ConditionKind Type { get; init; }
    }
}
