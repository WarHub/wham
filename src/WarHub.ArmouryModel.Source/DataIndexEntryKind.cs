using System.Xml.Serialization;

namespace WarHub.ArmouryModel.Source
{
    public enum DataIndexEntryKind
    {
        [XmlEnum("unknown")]
        Unknown,

        [XmlEnum("gamesystem")]
        Gamesystem,

        [XmlEnum("catalogue")]
        Catalogue,
    }
}
