using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("condition")]
    public sealed partial class ConditionCore : SelectorBaseCore
    {
        /// <summary>
        /// Changes the query to filter by this value.
        /// </summary>
        [XmlAttribute("childId")]
        public string ChildId { get; }

        [XmlAttribute("type")]
        public ConditionKind Type { get; }
    }
}
