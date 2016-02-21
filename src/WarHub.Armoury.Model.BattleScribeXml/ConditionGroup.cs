// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using GuidMapping;

    [XmlType("conditionGroup")]
    public sealed class ConditionGroup : GuidControllableBase, IGuidControllable
    {
        public ConditionGroup()
        {
            Type = ConditionGroupType.And;
            Conditions = new List<Condition>(0);
            ConditionGroups = new List<ConditionGroup>(0);
        }

        public ConditionGroup(ConditionGroup other)
        {
            Type = other.Type;
            Conditions = other.Conditions.Select(c => new Condition(c)).ToList();
            ConditionGroups = other.ConditionGroups.Select(g => new ConditionGroup(g)).ToList();
        }

        // ICondition inherited
        [XmlAttribute("type")]
        public ConditionGroupType Type { get; set; }

        [XmlArray("conditions")]
        public List<Condition> Conditions { get; set; }

        [XmlArray("conditionGroups")]
        public List<ConditionGroup> ConditionGroups { get; set; }

        public override void Process(GuidController controller)
        {
            base.Process(controller);
            controller.Process(Conditions);
            controller.Process(ConditionGroups);
        }
    }
}
