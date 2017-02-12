// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlRoot("catalogue", Namespace = "http://www.battlescribe.net/schema/catalogueSchema", IsNullable = false)]
    public partial class Catalogue : Datablob
    {
        [XmlAttribute("gameSystemId")]
        public string GameSystemId { get; set; }

        [XmlAttribute("gameSystemRevision")]
        public uint GameSystemRevision { get; set; }
    }
}
