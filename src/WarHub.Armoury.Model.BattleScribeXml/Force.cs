// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("force")]
    public sealed class Force : IdentifiedGuidControllableBase,
        IIdentified, IGuidControllable
    {
        private Guid _catalogueGuid;
        private Guid _forceTypeGuid;

        public Force()
        {
            Categories = new List<CategoryMock>(0);
            Forces = new List<Force>(0);
        }

        [XmlAttribute("id")]
        public override string Id { get; set; }

        [XmlAttribute("catalogueId")]
        public string CatalogueId { get; set; }

        [XmlIgnore]
        public Guid CatalogueGuid
        {
            get { return _catalogueGuid; }
            set { TrySetAndRaise(ref _catalogueGuid, value, newId => CatalogueId = newId); }
        }

        [XmlAttribute("catalogueRevision")]
        public uint CatalogueRevision { get; set; }

        [XmlAttribute("catalogueName")]
        public string CatalogueName { get; set; }

        [XmlAttribute("forceTypeId")]
        public string ForceTypeId { get; set; }

        [XmlIgnore]
        public Guid ForceTypeGuid
        {
            get { return _forceTypeGuid; }
            set { TrySetAndRaise(ref _forceTypeGuid, value, newId => ForceTypeId = newId); }
        }

        [XmlAttribute("forceTypeName")]
        public string ForceTypeName { get; set; }

        [XmlArray("categories")]
        public List<CategoryMock> Categories { get; set; }

        [XmlArray("forces")]
        public List<Force> Forces { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            CatalogueGuid = controller.ParseId(CatalogueId);
            ForceTypeGuid = controller.ParseId(ForceTypeId);
            controller.Process(Categories);
            controller.Process(Forces);
        }
    }
}
