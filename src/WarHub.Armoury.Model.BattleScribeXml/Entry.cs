// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("entry")]
    public sealed class Entry : IdentifiedGuidControllableBase, IEntryBase, IBookIndexed
    {
        private Guid _categoryGuid;

        public Entry()
        {
            Type = EntryType.Upgrade;
            MinSelections = 0;
            MaxSelections = -1;
            MinPoints = 0.0m;
            MaxPoints = -1.0m;
            MinInRoster = 0;
            MaxInRoster = -1;
            Entries = new List<Entry>(0);
            EntryGroups = new List<EntryGroup>(0);
            Modifiers = new List<Modifier>(0);
            Rules = new List<Rule>(0);
            Profiles = new List<Profile>(0);
            Links = new LinkList();
        }

        public Entry(Entry other)
        {
            Id = Guid.NewGuid().ToString();
            Name = other.Name;
            Points = other.Points;
            CategoryId = other.CategoryId;
            Type = other.Type;
            MinSelections = other.MinSelections;
            MaxSelections = other.MaxSelections;
            MinPoints = other.MinPoints;
            MaxPoints = other.MaxPoints;
            MinInRoster = other.MinInRoster;
            MaxInRoster = other.MaxInRoster;
            Collective = other.Collective;
            Book = other.Book;
            Page = other.Page;
            Entries = other.Entries.Select(e => new Entry(e)).ToList();
            EntryGroups = other.EntryGroups.Select(g => new EntryGroup(g)).ToList();
            Modifiers = other.Modifiers.Select(m => new Modifier(m)).ToList();
            Profiles = other.Profiles.Select(p => new Profile(p)).ToList();
            Rules = other.Rules.Select(r => new Rule(r)).ToList();
            Links = new LinkList(other.Links);
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("points")]
        public decimal Points { get; set; }

        [XmlAttribute("categoryId")]
        public string CategoryId { get; set; }

        [XmlIgnore]
        public Guid CategoryGuid
        {
            get { return _categoryGuid; }
            set { TrySetAndRaise(ref _categoryGuid, value, newId => CategoryId = newId); }
        }

        [XmlAttribute("type")]
        public EntryType Type { get; set; }

        /* IEntry inherited properties */

        [XmlAttribute("minSelections")]
        public int MinSelections { get; set; }

        [XmlAttribute("maxSelections")]
        public int MaxSelections { get; set; }

        [XmlAttribute("minInForce")]
        public int MinInForce { get; set; }

        [XmlAttribute("maxInForce")]
        public int MaxInForce { get; set; }

        [XmlAttribute("minInRoster")]
        public int MinInRoster { get; set; }

        [XmlAttribute("maxInRoster")]
        public int MaxInRoster { get; set; }

        [XmlAttribute("minPoints")]
        public decimal MinPoints { get; set; }

        [XmlAttribute("maxPoints")]
        public decimal MaxPoints { get; set; }

        [XmlAttribute("collective")]
        public bool Collective { get; set; }

        [XmlAttribute("hidden")]
        public bool Hidden { get; set; }

        /* IBookIndexable inherited */

        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlAttribute("page")]
        public string Page { get; set; }

        /* content nodes */

        [XmlArray("entries")]
        public List<Entry> Entries { get; set; }

        [XmlArray("entryGroups")]
        public List<EntryGroup> EntryGroups { get; set; }

        [XmlArray("modifiers")]
        public List<Modifier> Modifiers { get; set; }

        [XmlArray("rules")]
        public List<Rule> Rules { get; set; }

        [XmlArray("profiles")]
        public List<Profile> Profiles { get; set; }

        [XmlArray("links")]
        public LinkList Links { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            CategoryGuid = controller.ParseId(CategoryId);
            controller.Process(Entries);
            controller.Process(EntryGroups);
            controller.Process(Links);
            controller.Process(Modifiers);
            controller.Process(Profiles);
            controller.Process(Rules);
        }
    }
}
