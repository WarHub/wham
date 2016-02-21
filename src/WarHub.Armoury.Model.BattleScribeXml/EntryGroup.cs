// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("entryGroup")]
    public sealed class EntryGroup : IdentifiedGuidControllableBase, IEntryBase
    {
        private Guid _defaultEntryGuid;

        public EntryGroup()
        {
            Id = null;
            Name = null;
            MinSelections = 0;
            MaxSelections = -1;
            MinPoints = 0.0m;
            MaxPoints = -1.0m;
            MinInRoster = 0;
            MaxInRoster = -1;
            Collective = false;
            Entries = new List<Entry>();
            EntryGroups = new List<EntryGroup>();
            Modifiers = new List<Modifier>();
            Links = new LinkList();
        }

        public EntryGroup(EntryGroup other)
        {
            Id = Guid.NewGuid().ToString();
            Name = other.Name;
            MinSelections = other.MinSelections;
            MaxSelections = other.MaxSelections;
            MinPoints = other.MinPoints;
            MaxPoints = other.MaxPoints;
            MinInRoster = other.MinInRoster;
            MaxInRoster = other.MaxInRoster;
            Collective = other.Collective;
            Entries = other.Entries.Select(e => new Entry(e)).ToList();
            EntryGroups = other.EntryGroups.Select(g => new EntryGroup(g)).ToList();
            Modifiers = other.Modifiers.Select(m => new Modifier(m)).ToList();
            Links = new LinkList(other.Links);
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("defaultEntryId")]
        public string DefaultEntryId { get; set; }

        [XmlIgnore]
        public Guid DefaultEntryGuid
        {
            get { return _defaultEntryGuid; }
            set { TrySetAndRaise(ref _defaultEntryGuid, value, newId => DefaultEntryId = newId); }
        }

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

        /* content nodes */

        [XmlArray("entries")]
        public List<Entry> Entries { get; set; }

        [XmlArray("entryGroups")]
        public List<EntryGroup> EntryGroups { get; set; }

        [XmlArray("modifiers")]
        public List<Modifier> Modifiers { get; set; }

        [XmlArray("links")]
        public LinkList Links { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            DefaultEntryGuid = controller.ParseId(DefaultEntryId);
            controller.Process(Entries);
            controller.Process(EntryGroups);
            controller.Process(Links);
            controller.Process(Modifiers);
        }
    }
}
