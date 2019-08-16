using System.Xml.Serialization;
using WarHub.ArmouryModel.Source.XmlFormat;

namespace WarHub.ArmouryModel.Source
{
    [WhamNodeCore]
    [XmlRoot(RootElementNames.GameSystem, Namespace = Namespaces.GamesystemXmlns, IsNullable = false)]
    public partial class GamesystemCore : CatalogueBaseCore
    {
    }
}
