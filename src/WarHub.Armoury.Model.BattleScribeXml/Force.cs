// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlType("force")]
    public class Force : RosterElementBase
    {
        [XmlAttribute("catalogueId")]
        public string CatalogueId { get; set; }

        [XmlAttribute("catalogueRevision")]
        public uint CatalogueRevision { get; set; }

        [XmlAttribute("catalogueName")]
        public string CatalogueName { get; set; }

        [XmlArray("categories", Order = 2)]
        public Category[] Categories { get; set; }

        [XmlArray("forces", Order = 3)]
        public Force[] Forces { get; set; }
    }
}
