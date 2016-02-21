// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("rule")]
    public sealed class Rule : IdentifiedGuidControllableBase,
        IIdentified, INamed, IBookIndexed, IGuidControllable
    {
        public Rule()
        {
            Modifiers = new List<Modifier>(0);
        }

        public Rule(Rule other)
        {
            Id = Guid.NewGuid().ToString();
            Name = other.Name;
            Hidden = other.Hidden;
            Book = other.Book;
            Page = other.Page;
            Description = other.Description;
            Modifiers = other.Modifiers.Select(m => new Modifier(m)).ToList();
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("hidden")]
        public bool Hidden { get; set; }

        /* IBookIndexable inherited */

        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlAttribute("page")]
        public string Page { get; set; }

        /* content nodes */

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlArray("modifiers")]
        public List<Modifier> Modifiers { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            controller.Process(Modifiers);
        }
    }
}
