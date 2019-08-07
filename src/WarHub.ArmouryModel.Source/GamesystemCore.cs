using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(
        XmlInformation.RootElementNames.GameSystem,
        Namespace = XmlInformation.Namespaces.GamesystemXmlns,
        IsNullable = false)]
    public partial class GamesystemCore : CatalogueBaseCore
    {
    }
}
