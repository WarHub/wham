// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using BattleScribeXml;
    using Nodes;

    public class CategoryModifier
        : Modifier<decimal, CategoryModifierAction, LimitField>, ICategoryModifier
    {
        private readonly GameSystemConditionGroupNode _conditionGroups;
        private readonly GameSystemConditionNode _conditions;
        private IGameSystemContext _context;

        public CategoryModifier(Modifier xml)
            : base(xml)
        {
            InitField();
            InitType();
            _conditions = new GameSystemConditionNode(() => XmlBackend.Conditions, this)
            {
                Controller = XmlBackend.Controller
            };
            _conditionGroups = new GameSystemConditionGroupNode(
                () => XmlBackend.ConditionGroups,
                this) {Controller = XmlBackend.Controller};
        }

        public INodeSimple<IGameSystemConditionGroup> ConditionGroups
        {
            get { return _conditionGroups; }
        }

        public INodeSimple<IGameSystemCondition> Conditions
        {
            get { return _conditions; }
        }

        public IGameSystemContext Context
        {
            get { return _context; }
            set
            {
                if (Set(ref _context, value))
                {
                    Conditions.ChangeContext(value);
                    ConditionGroups.ChangeContext(value);
                }
            }
        }

        public override decimal Value
        {
            get { return decimal.Parse(XmlBackend.Value); }
            set
            {
                Set(XmlBackend.Value,
                    value.ToString(),
                    () => XmlBackend.Value = value.ToString());
            }
        }

        public ICategoryModifier Clone()
        {
            return new CategoryModifier(new Modifier(XmlBackend));
        }
    }
}
