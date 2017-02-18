namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlInclude(typeof(EntryLink))]
    [XmlInclude(typeof(SelectionEntryGroup))]
    [XmlInclude(typeof(SelectionEntry))]
    [XmlInclude(typeof(ForceEntry))]
    [XmlInclude(typeof(CategoryEntry))]
    [XmlInclude(typeof(InfoLink))]
    [XmlInclude(typeof(Rule))]
    [XmlInclude(typeof(Profile))]
    public partial class EntryBase
    {
        [XmlArray("profiles", Order = 0)]
        public List<Profile> Profiles { get; } = new List<Profile>(0);

        [XmlArray("rules", Order = 1)]
        public List<Rule> Rules { get; } = new List<Rule>(0);

        [XmlArray("infoLinks", Order = 2)]
        public List<InfoLink> InfoLinks { get; } = new List<InfoLink>(0);

        [XmlArray("modifiers", Order = 3)]
        public List<Modifier> Modifiers { get; } = new List<Modifier>(0);

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlAttribute("page")]
        public string Page { get; set; }

        [XmlAttribute("hidden")]
        public bool Hidden { get; set; }
    }
}