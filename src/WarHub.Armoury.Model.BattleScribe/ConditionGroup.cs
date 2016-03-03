// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;

    public class ConditionGroup : XmlBackedModelBase<BattleScribeXml.ConditionGroup>, IConditionGroup
    {
        protected ConditionGroup(BattleScribeXml.ConditionGroup xml)
            : base(xml)
        {
        }

        public IConditionGroup Clone()
        {
            return new ConditionGroup(new BattleScribeXml.ConditionGroup(XmlBackend));
        }

        public ConditionGroupType Type
        {
            get { return XmlBackend.Type; }
            set { Set(XmlBackend.Type, value, () => XmlBackend.Type = value); }
        }
    }
}
