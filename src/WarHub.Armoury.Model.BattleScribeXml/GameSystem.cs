// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlRoot("gameSystem", Namespace = BsGameSystemXmlNamespace)]
    public class GameSystem : IdentifiedGuidControllableBase, IXmlProperties, ICatalogue
    {
        public const string BsGameSystemXmlNamespace =
            "http://www.battlescribe.net/schema/gameSystemSchema";

        public GameSystem()
        {
            ForceTypes = new List<ForceType>(0);
            ProfileTypes = new List<ProfileType>(0);
        }

        public string DefaultXmlNamespace => BsGameSystemXmlNamespace;

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("revision")]
        public uint Revision { get; set; }

        [XmlAttribute("battleScribeVersion")]
        public string BattleScribeVersion { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("books")]
        public string Books { get; set; }

        /* Author details */

        [XmlAttribute("authorName")]
        public string AuthorName { get; set; }

        [XmlAttribute("authorContact")]
        public string AuthorContact { get; set; }

        [XmlAttribute("authorUrl")]
        public string AuthorUrl { get; set; }

        /* content nodes */

        [XmlArray("forceTypes")]
        public List<ForceType> ForceTypes { get; set; }

        [XmlArray("profileTypes")]
        public List<ProfileType> ProfileTypes { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            controller.Process(ForceTypes);
            controller.Process(ProfileTypes);
        }
    }
}
