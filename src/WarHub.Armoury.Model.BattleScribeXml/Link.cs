// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("link")]
    public sealed class Link : IdentifiedGuidControllableBase, IIdentified, IGuidControllable
    {
        private Guid _targetGuid;
        private Guid _categoryGuid;

        public Link()
        {
            Id = string.Empty;
            TargetId = string.Empty;
            LinkType = LinkType.Entry;
            Modifiers = new List<Modifier>(0);
        }

        public Link(Link other)
        {
            Id = Guid.NewGuid().ToString();
            TargetId = other.TargetId;
            LinkType = other.LinkType;
            CategoryId = other.CategoryId;
            Modifiers = other.Modifiers.Select(x => new Modifier(x)).ToList();
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("targetId")]
        public string TargetId { get; set; }

        [XmlIgnore]
        public Guid TargetGuid
        {
            get { return _targetGuid; }
            set { TrySetAndRaise(ref _targetGuid, value, newId => TargetId = newId); }
        }

        [XmlAttribute("linkType")]
        public LinkType LinkType { get; set; }

        [XmlAttribute("categoryId")]
        public string CategoryId { get; set; }

        [XmlIgnore]
        public Guid CategoryGuid
        {
            get { return _categoryGuid; }
            set { TrySetAndRaise(ref _categoryGuid, value, newId => CategoryId = newId); }
        }

        /* content nodes */

        [XmlArray("modifiers")]
        public List<Modifier> Modifiers { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            TargetGuid = controller.ParseId(TargetId);
            CategoryGuid = controller.ParseId(CategoryId);
            if (LinkType == LinkType.Profile)
            {
                foreach (var modifier in Modifiers)
                {
                    modifier.FieldCharacteristicGuid = controller.ParseId(modifier.Field);
                }
            }
            controller.Process(Modifiers);
        }
    }
}
