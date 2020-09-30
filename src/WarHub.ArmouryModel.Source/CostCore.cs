using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("cost")]
    public sealed partial record CostCore : CostBaseCore
    {
    }
}
