// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlInclude(typeof(GameSystem))]
    [XmlInclude(typeof(Catalogue))]
    public partial class Datablob
    {
        [XmlArray("profiles", Order = 0)]
        public List<Profile> Profiles { get; } = new List<Profile>(0);

        [XmlArray("rules", Order = 1)]
        public List<Rule> Rules { get; } = new List<Rule>(0);

        [XmlArray("infoLinks", Order = 2)]
        public List<InfoLink> InfoLinks { get; } = new List<InfoLink>(0);

        [XmlArray("costTypes", Order = 3)]
        public List<CostType> CostTypes { get; } = new List<CostType>(0);

        [XmlArray("profileTypes", Order = 4)]
        public List<ProfileType> ProfileTypes { get; } = new List<ProfileType>(0);

        [XmlArray("forceEntries", Order = 5)]
        public List<ForceEntry> ForceEntries { get; } = new List<ForceEntry>(0);

        [XmlArray("selectionEntries", Order = 6)]
        public List<SelectionEntry> SelectionEntries { get; } = new List<SelectionEntry>(0);

        [XmlArray("entryLinks", Order = 7)]
        public List<EntryLink> EntryLinks { get; } = new List<EntryLink>(0);

        [XmlArray("sharedSelectionEntries", Order = 8)]
        public List<SelectionEntry> SharedSelectionEntries { get; } = new List<SelectionEntry>(0);

        [XmlArray("sharedSelectionEntryGroups", Order = 9)]
        public List<SelectionEntryGroup> SharedSelectionEntryGroups { get; } = new List<SelectionEntryGroup>(0);

        [XmlArray("sharedRules", Order = 10)]
        public List<Rule> SharedRules { get; } = new List<Rule>(0);

        [XmlArray("sharedProfiles", Order = 11)]
        public List<Profile> SharedProfiles { get; } = new List<Profile>(0);

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlAttribute("page")]
        public string Page { get; set; }

        [XmlAttribute("revision")]
        public uint Revision { get; set; }

        [XmlAttribute("battleScribeVersion")]
        public string BattleScribeVersion { get; set; }

        [XmlAttribute("authorName")]
        public string AuthorName { get; set; }

        [XmlAttribute("authorContact")]
        public string AuthorContact { get; set; }

        [XmlAttribute("authorUrl")]
        public string AuthorUrl { get; set; }
    }
}
