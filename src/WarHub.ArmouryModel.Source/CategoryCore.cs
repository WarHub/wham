using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("category")]
    public sealed partial record CategoryCore : RosterElementBaseCore
    {
        [XmlAttribute("primary")]
        public bool Primary { get; init; }
    }
}
