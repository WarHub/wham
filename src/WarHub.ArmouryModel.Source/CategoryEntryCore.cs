using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlType("categoryEntry")]
    public sealed partial record CategoryEntryCore : ContainerEntryBaseCore
    {
    }
}
