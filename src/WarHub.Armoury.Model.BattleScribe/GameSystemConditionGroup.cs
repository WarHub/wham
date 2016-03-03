namespace WarHub.Armoury.Model.BattleScribe
{
    using Nodes;

    public class GameSystemConditionGroup : ConditionGroup, IGameSystemConditionGroup
    {
        private readonly GameSystemConditionGroupNode _conditionGroups;
        private readonly GameSystemConditionNode _conditions;
        private IGameSystemContext _context;

        public GameSystemConditionGroup(BattleScribeXml.ConditionGroup xml)
            : base(xml)
        {
            _conditions = new GameSystemConditionNode(() => XmlBackend.Conditions, this);
            _conditionGroups = new GameSystemConditionGroupNode(
                () => XmlBackend.ConditionGroups,
                this);
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
    }
}
