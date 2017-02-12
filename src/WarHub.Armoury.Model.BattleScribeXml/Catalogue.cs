// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlRoot("catalogue", Namespace = CatalogueXmlNamespace, IsNullable = false)]
    public class Catalogue : Datablob, IXmlProperties
    {
        public const string CatalogueXmlNamespace = "http://www.battlescribe.net/schema/catalogueSchema";

        public string DefaultXmlNamespace => CatalogueXmlNamespace;

        [XmlAttribute("gameSystemId")]
        public string GameSystemId { get; set; }

        [XmlAttribute("gameSystemRevision")]
        public uint GameSystemRevision { get; set; }
    }
}
