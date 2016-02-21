// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("profile")]
    public sealed class Profile : IdentifiedGuidControllableBase,
        IIdentified, INamed, IBookIndexed, IGuidControllable
    {
        private Guid _profileTypeGuid;

        public Profile()
        {
            ProfileTypeId = string.Empty; // TODO profileTypeId in Profile must be populated!
            Characteristics = new List<Characteristic>();
            Modifiers = new List<Modifier>(0);
        }

        public Profile(Profile other)
        {
            Id = Guid.NewGuid().ToString();
            Name = other.Name;
            Book = other.Book;
            Page = other.Page;
            Hidden = other.Hidden;
            ProfileTypeId = other.ProfileTypeId;
            Characteristics = other.Characteristics.Select(ch => new Characteristic(ch)).ToList();
            Modifiers = other.Modifiers.Select(m => new Modifier(m)).ToList();
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("profileTypeId")]
        public string ProfileTypeId { get; set; }

        [XmlIgnore]
        public Guid ProfileTypeGuid
        {
            get { return _profileTypeGuid; }
            set { TrySetAndRaise(ref _profileTypeGuid, value, newId => ProfileTypeId = newId); }
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("hidden")]
        public bool Hidden { get; set; }

        [XmlAttribute("book")]
        public string Book { get; set; }

        [XmlAttribute("page")]
        public string Page { get; set; }

        /* content nodes */

        [XmlArray("characteristics")]
        public List<Characteristic> Characteristics { get; set; }

        [XmlArray("modifiers")]
        public List<Modifier> Modifiers { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            ProfileTypeGuid = controller.ParseId(ProfileTypeId);
            foreach (var modifier in Modifiers)
            {
                modifier.FieldCharacteristicGuid = controller.ParseId(modifier.Field);
            }
            controller.Process(Characteristics);
            controller.Process(Modifiers);
        }
    }
}
