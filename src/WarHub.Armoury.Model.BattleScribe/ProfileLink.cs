namespace WarHub.Armoury.Model.BattleScribe
{
    using Nodes;

    public class ProfileLink : ModifiableLink<IProfile, IProfileModifier>, IProfileLink
    {
        private ICatalogueContext _context;

        public ProfileLink(BattleScribeXml.Link xml)
            : base(xml)
        {
            Modifiers = new ProfileModifierNode(() => XmlBackend.Modifiers, this) {Controller = XmlBackend.Controller};
        }

        public override ICatalogueContext Context
        {
            get { return _context; }
            set
            {
                var old = _context;
                if (!Set(ref _context, value))
                    return;
                old?.ProfileLinks.Deregister(this);
                Target = null;
                if (value != null)
                {
                    value.ProfileLinks.Register(this);
                    value.Profiles.SetTargetOf(this);
                }
                Modifiers.ChangeContext(value);
            }
        }

        public override INodeSimple<IProfileModifier> Modifiers { get; }
    }
}
