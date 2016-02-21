// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("category")]
    public sealed class CategoryMock : IdentifiedGuidControllableBase,
        IIdentified, INamed, IGuidControllable
    {
        private Guid _categoryGuid;

        public CategoryMock()
        {
            Selections = new List<Selection>();
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("categoryId")]
        public string CategoryId { get; set; }

        [XmlIgnore]
        public Guid CategoryGuid
        {
            get { return _categoryGuid; }
            set { TrySetAndRaise(ref _categoryGuid, value, newId => CategoryId = newId); }
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlArray("selections")]
        public List<Selection> Selections { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            CategoryGuid = controller.ParseId(CategoryId);
            controller.Process(Selections);
        }
    }
}
