namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;
    using Nodes;

    public class Roster : IdentifiedNamedModelBase<BattleScribeXml.Roster>, IRoster
    {
        private readonly ForceNode _forcesNode;
        private readonly IdLink<IGameSystem> _gameSystemLink;
        private IRosterContext _context;
        private IGameSystemContext _systemContext;

        public Roster(BattleScribeXml.Roster xml)
            : base(xml)
        {
            _forcesNode = new ForceNode(() => XmlBackend.Forces, this) {Controller = XmlBackend.Controller};
            _gameSystemLink = new IdLink<IGameSystem>(
                XmlBackend.GameSystemGuid,
                newGuid => XmlBackend.GameSystemGuid = newGuid,
                () => XmlBackend.GameSystemId);
        }

        public IRosterContext Context
        {
            get { return _context; }
            private set
            {
                var oldValue = _context;
                if (!Set(ref _context, value))
                {
                    return;
                }
                if (oldValue != null)
                {
                    oldValue.PointCostChanged -= OnRosterContextPointCostChanged;
                }
                Forces.ChangeContext(value);
                if (value != null)
                {
                    value.PointCostChanged += OnRosterContextPointCostChanged;
                }
            }
        }

        public INode<IForce, ForceNodeArgument> Forces
        {
            get { return _forcesNode; }
        }

        public IIdLink<IGameSystem> GameSystemLink
        {
            get { return _gameSystemLink; }
        }

        public string GameSystemName
        {
            get { return XmlBackend.GameSystemName; }
        }

        public uint GameSystemRevision
        {
            get { return XmlBackend.GameSystemRevision; }
        }

        public string OriginProgramVersion
        {
            get { return XmlBackend.BattleScribeVersion; }
        }

        public decimal PointCost
        {
            get { return XmlBackend.Points; }
            protected set { Set(XmlBackend.Points, value, () => XmlBackend.Points = value); }
        }

        public decimal PointsLimit
        {
            get { return XmlBackend.PointsLimit; }
            set { Set(XmlBackend.PointsLimit, value, () => XmlBackend.PointsLimit = value); }
        }

        public IGameSystemContext SystemContext
        {
            get { return _systemContext; }
            set
            {
                if (Set(ref _systemContext, value))
                {
                    if (value != null)
                    {
                        GameSystemLink.Target = value.GameSystem;
                        Context = new RosterContext(this);
                    }
                    else
                    {
                        Context = null;
                        GameSystemLink.Target = null;
                    }
                }
            }
        }

        protected virtual void OnRosterContextPointCostChanged(object sender, PointCostChangedEventArgs e)
        {
            PointCost = this.GetTotalPoints();
        }
    }
}
