namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlInclude(typeof(GameSystem))]
    [XmlInclude(typeof(Catalogue))]
    public class Datablob
    {
        [XmlArray("profiles", Order = 0)]
        public Profile[] Profiles { get; set; }

        [XmlArray("rules", Order = 1)]
        public Rule[] Rules { get; set; }

        [XmlArray("infoLinks", Order = 2)]
        public InfoLink[] InfoLinks { get; set; }

        [XmlArray("costTypes", Order = 3)]
        public CostType[] CostTypes { get; set; }

        [XmlArray("profileTypes", Order = 4)]
        public ProfileType[] ProfileTypes { get; set; }

        [XmlArray("forceEntries", Order = 5)]
        public ForceEntry[] ForceEntries { get; set; }

        [XmlArray("selectionEntries", Order = 6)]
        public SelectionEntry[] SelectionEntries { get; set; }

        [XmlArray("entryLinks", Order = 7)]
        public EntryLink[] EntryLinks { get; set; }

        [XmlArray("sharedSelectionEntries", Order = 8)]
        public SelectionEntry[] SharedSelectionEntries { get; set; }

        [XmlArray("sharedSelectionEntryGroups", Order = 9)]
        public SelectionEntryGroup[] SharedSelectionEntryGroups { get; set; }

        [XmlArray("sharedRules", Order = 10)]
        public Rule[] SharedRules { get; set; }

        [XmlArray("sharedProfiles", Order = 11)]
        public Profile[] SharedProfiles { get; set; }

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