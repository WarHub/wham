// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("condition")]
    public sealed class Condition : GuidControllableBase, IGuidControllable
    {
        private Guid _parentGuid;
        private Guid _childGuid;

        public Condition()
        {
            Value = 0.0m;
            Field = ConditionValueUnit.Points;
            ParentId = ReservedIdentifiers.RosterAncestorName;
            ChildId = null;
            Type = ConditionKind.EqualTo;
        }

        public Condition(Condition other)
        {
            Value = other.Value;
            Field = other.Field;
            ParentId = other.ParentId;
            ChildId = other.ChildId;
            Type = other.Type;
        }

        [XmlAttribute("parentId")]
        public string ParentId { get; set; }

        [XmlIgnore]
        public Guid ParentGuid
        {
            get { return _parentGuid; }
            set { TrySetAndRaise(ref _parentGuid, value, newId => ParentId = newId); }
        }

        [XmlAttribute("childId")]
        public string ChildId { get; set; }

        [XmlIgnore]
        public Guid ChildGuid
        {
            get { return _childGuid; }
            set { TrySetAndRaise(ref _childGuid, value, newId => ChildId = newId); }
        }

        [XmlAttribute("field")]
        public ConditionValueUnit Field { get; set; }

        [XmlAttribute("type")]
        public ConditionKind Type { get; set; }

        [XmlAttribute("value")]
        public decimal Value { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            ChildGuid = controller.ParseId(ChildId);
            ParentGuid = controller.ParseId(ParentId);
        }
    }
}
