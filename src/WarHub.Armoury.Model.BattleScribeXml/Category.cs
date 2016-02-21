// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("category")]
    public sealed class Category : IdentifiedGuidControllableBase,
        IIdentified, INamed, IGuidControllable
    {
        public Category()
        {
            MinSelections = MinPercentage = 0;
            MaxSelections = MaxPercentage = -1;
            MinPoints = 0.0m;
            MaxPoints = -1.0m;
            Modifiers = new List<Modifier>(0);
        }

        public Category(Category other, string newGuid)
        {
            Id = newGuid;
            Name = other.Name;
            MinSelections = other.MinSelections;
            MaxSelections = other.MaxSelections;
            MinPoints = other.MinPoints;
            MaxPoints = other.MaxPoints;
            MinPercentage = other.MaxPercentage;
            CountTowardsParentMinSelections = other.CountTowardsParentMinSelections;
            CountTowardsParentMaxSelections = other.CountTowardsParentMaxSelections;
            CountTowardsParentMinPoints = other.CountTowardsParentMinPoints;
            CountTowardsParentMaxPoints = other.CountTowardsParentMaxPoints;
            CountTowardsParentMinPercentage = other.CountTowardsParentMinPercentage;
            CountTowardsParentMaxPercentage = other.CountTowardsParentMaxPercentage;
            Modifiers = other.Modifiers.TransCreate(m => new Modifier(m));
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("minSelections")]
        public int MinSelections { get; set; }

        [XmlAttribute("maxSelections")]
        public int MaxSelections { get; set; }

        [XmlAttribute("minPoints")]
        public decimal MinPoints { get; set; }

        [XmlAttribute("maxPoints")]
        public decimal MaxPoints { get; set; }

        [XmlAttribute("minPercentage")]
        public int MinPercentage { get; set; }

        [XmlAttribute("maxPercentage")]
        public int MaxPercentage { get; set; }

        [XmlAttribute("countTowardsParentMinSelections")]
        public bool CountTowardsParentMinSelections { get; set; }

        [XmlAttribute("countTowardsParentMaxSelections")]
        public bool CountTowardsParentMaxSelections { get; set; }

        [XmlAttribute("countTowardsParentMinPoints")]
        public bool CountTowardsParentMinPoints { get; set; }

        [XmlAttribute("countTowardsParentMaxPoints")]
        public bool CountTowardsParentMaxPoints { get; set; }

        [XmlAttribute("countTowardsParentMinPercentage")]
        public bool CountTowardsParentMinPercentage { get; set; }

        [XmlAttribute("countTowardsParentMaxPercentage")]
        public bool CountTowardsParentMaxPercentage { get; set; }

        [XmlArray("modifiers")]
        public List<Modifier> Modifiers { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            controller.Process(Modifiers);
        }
    }
}
