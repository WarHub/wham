namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;
    using Nodes;

    public class GameSystem : IdentifiedNamedModelBase<BattleScribeXml.GameSystem>, IGameSystem
    {
        private readonly AuthorDetails _author;
        private readonly ForceTypeNode _forceTypes;
        private readonly ProfileTypeNode _profileTypes;

        public GameSystem(BattleScribeXml.GameSystem xml)
            : base(xml)
        {
            Context = new GameSystemContext(this);
            _author = new AuthorDetails(xml);
            _forceTypes = new ForceTypeNode(() => XmlBackend.ForceTypes, this) {Controller = XmlBackend.Controller};
            _profileTypes = new ProfileTypeNode(() => XmlBackend.ProfileTypes, this)
            {
                Controller = XmlBackend.Controller
            };
            SetContext();
        }

        public IAuthorDetails Author
        {
            get { return _author; }
        }

        public string BookSources
        {
            get { return XmlBackend.Books; }
            set { Set(XmlBackend.Books, value, newValue => XmlBackend.Books = newValue); }
        }

        public IGameSystemContext Context { get; set; }

        public INodeSimple<IForceType> ForceTypes
        {
            get { return _forceTypes; }
        }

        public string OriginProgramVersion
        {
            get { return XmlBackend.BattleScribeVersion; }
        }

        public INodeSimple<IProfileType> ProfileTypes
        {
            get { return _profileTypes; }
        }

        public uint Revision
        {
            get { return XmlBackend.Revision; }
            set { Set(XmlBackend.Revision, value, newValue => XmlBackend.Revision = newValue); }
        }

        private void SetContext()
        {
            ForceTypes.ChangeContext(Context);
            ProfileTypes.ChangeContext(Context);
        }
    }
}
