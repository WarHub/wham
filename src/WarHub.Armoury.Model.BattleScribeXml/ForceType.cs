// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("forceType")]
    public sealed class ForceType : IdentifiedGuidControllableBase,
        IIdentified, INamed, IGuidControllable
    {
        public ForceType()
        {
            MinSelections = MinPercentage = 0;
            MaxSelections = MaxPercentage = -1;
            MinPoints = 0.0m;
            MaxPoints = -1.0m;
            Categories = new List<Category>(0);
            ForceTypes = new List<ForceType>(0);
        }

        public ForceType(ForceType other)
        {
            Id = Guid.NewGuid().ToString(GuidController.GuidFormat);
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
            Categories = other.Categories
                .Select(cat => new Category(cat, Guid.NewGuid().ToString(GuidController.GuidFormat)))
                .ToList();
            ForceTypes = other.ForceTypes
                .Select(ft => new ForceType(ft))
                .ToList();
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

        [XmlArray("categories")]
        public List<Category> Categories { get; set; }

        [XmlArray("forceTypes")]
        public List<ForceType> ForceTypes { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            controller.Process(Categories);
            controller.Process(ForceTypes);
        }
    }
}
