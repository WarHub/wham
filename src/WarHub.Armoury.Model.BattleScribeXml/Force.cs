// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
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
        public List<Category> Categories { get; } = new List<Category>(0);

        [XmlArray("forces", Order = 3)]
        public List<Force> Forces { get; } = new List<Force>(0);
    }
}
