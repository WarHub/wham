// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlRoot("catalogue", Namespace = BsCatalogueXmlNamespace)]
    public sealed class Catalogue : IdentifiedGuidControllableBase,
        IXmlProperties, ICatalogue
    {
        public const string BsCatalogueXmlNamespace =
            "http://www.battlescribe.net/schema/catalogueSchema";

        private Guid _gameSystemGuid;

        public Catalogue()
        {
            Entries = new List<Entry>(0);
            Rules = new List<Rule>(0);
            Links = new LinkList();
            SharedEntries = new List<Entry>(0);
            SharedEntryGroups = new List<EntryGroup>(0);
            SharedRules = new List<Rule>(0);
            SharedProfiles = new List<Profile>(0);
        }

        public string DefaultXmlNamespace => BsCatalogueXmlNamespace;

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("revision")]
        public uint Revision { get; set; }

        [XmlAttribute("gameSystemId")]
        public string GameSystemId { get; set; }

        [XmlIgnore]
        public Guid GameSystemGuid
        {
            get { return _gameSystemGuid; }
            set { TrySetAndRaise(ref _gameSystemGuid, value, newId => GameSystemId = newId); }
        }

        [XmlAttribute("gameSystemRevision")]
        public uint GameSystemRevision { get; set; }

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

        [XmlArray("entries")]
        public List<Entry> Entries { get; set; }

        [XmlArray("rules")]
        public List<Rule> Rules { get; set; }

        [XmlArray("links")]
        public LinkList Links { get; set; }

        /* content nodes (shared)    */

        [XmlArray("sharedEntries")]
        public List<Entry> SharedEntries { get; set; }

        [XmlArray("sharedEntryGroups")]
        public List<EntryGroup> SharedEntryGroups { get; set; }

        [XmlArray("sharedRules")]
        public List<Rule> SharedRules { get; set; }

        [XmlArray("sharedProfiles")]
        public List<Profile> SharedProfiles { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            GameSystemGuid = controller.ParseId(GameSystemId);
            controller.Process(Entries);
            controller.Process(Links);
            controller.Process(Rules);
            controller.Process(SharedEntries);
            controller.Process(SharedEntryGroups);
            controller.Process(SharedProfiles);
            controller.Process(SharedRules);
        }
    }
}
