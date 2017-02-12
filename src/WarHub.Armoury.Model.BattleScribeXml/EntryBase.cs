namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Xml.Serialization;

    [XmlInclude(typeof(EntryLink))]
    [XmlInclude(typeof(SelectionEntryGroup))]
    [XmlInclude(typeof(SelectionEntry))]
    [XmlInclude(typeof(ForceEntry))]
    [XmlInclude(typeof(CategoryEntry))]
    [XmlInclude(typeof(InfoLink))]
    [XmlInclude(typeof(Rule))]
    [XmlInclude(typeof(Profile))]
    public class EntryBase
    {
        [XmlArray("profiles", Order = 0)]
        public Profile[] Profiles { get; set; }

        [XmlArray("rules", Order = 1)]
        public Rule[] Rules { get; set; }

        [XmlArray("infoLinks", Order = 2)]
        public InfoLink[] InfoLinks { get; set; }

        [XmlArray("modifiers", Order = 3)]
        public Modifier[] Modifiers { get; set; }

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