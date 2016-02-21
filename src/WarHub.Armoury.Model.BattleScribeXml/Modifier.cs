// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("modifier")]
    public sealed class Modifier : GuidControllableBase
    {
        private Guid _repeatParentGuid;
        private Guid _repeatChildGuid;
        private Guid _fieldCharacteristicGuid;
        /* properties */

        public Modifier()
        {
            Value = string.Empty;
            NumberOfRepeats = 1;
            RepeatValue = 1.0m;
            Conditions = new List<Condition>(0);
            ConditionGroups = new List<ConditionGroup>(0);
        }

        public Modifier(Modifier other)
        {
            Type = other.Type;
            Field = other.Field;
            Value = other.Value;
            Repeating = other.Repeating;
            NumberOfRepeats = other.NumberOfRepeats;
            RepeatValue = other.RepeatValue;
            RepeatField = other.RepeatField;
            RepeatParentId = other.RepeatParentId;
            RepeatChildId = other.RepeatChildId;
            Conditions = other.Conditions.TransCreate(c => new Condition(c));
            ConditionGroups = other.ConditionGroups.TransCreate(g => new ConditionGroup(g));
        }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("field")]
        public string Field { get; set; }

        [XmlIgnore]
        public Guid FieldCharacteristicGuid
        {
            get { return _fieldCharacteristicGuid; }
            set { TrySetAndRaise(ref _fieldCharacteristicGuid, value, newId => Field = newId); }
        }

        [XmlAttribute("value")]
        public string Value { get; set; }

        /* repeating block */

        [XmlAttribute("repeat")]
        public bool Repeating { get; set; }

        [XmlAttribute("numRepeats")]
        public uint NumberOfRepeats { get; set; }

        [XmlAttribute("incrementParentId")]
        public string RepeatParentId { get; set; }

        [XmlIgnore]
        public Guid RepeatParentGuid
        {
            get { return _repeatParentGuid; }
            set { TrySetAndRaise(ref _repeatParentGuid, value, newId => RepeatParentId = newId); }
        }

        [XmlAttribute("incrementChildId")]
        public string RepeatChildId { get; set; }

        [XmlIgnore]
        public Guid RepeatChildGuid
        {
            get { return _repeatChildGuid; }
            set { TrySetAndRaise(ref _repeatChildGuid, value, newId => RepeatChildId = newId); }
        }

        [XmlAttribute("incrementField")]
        public ConditionValueUnit RepeatField { get; set; }

        [XmlAttribute("incrementValue")]
        public decimal RepeatValue { get; set; }

        /* condition nodes */

        [XmlArray("conditions")]
        public List<Condition> Conditions { get; set; }

        [XmlArray("conditionGroups")]
        public List<ConditionGroup> ConditionGroups { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            RepeatChildGuid = controller.ParseId(RepeatChildId);
            RepeatParentGuid = controller.ParseId(RepeatParentId);
            // this is moved to profile and profile link
            //FieldCharacteristicGuid = controller.ParseId(Field);
            controller.Process(Conditions);
            controller.Process(ConditionGroups);
        }
    }
}
