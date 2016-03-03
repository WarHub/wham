namespace WarHub.Armoury.Model.BattleScribe
{
    using Nodes;

    public class GroupLink : ModifiableLink<IGroup, IGroupModifier>, IGroupLink
    {
        private ICatalogueContext _context;

        public GroupLink(BattleScribeXml.Link xml)
            : base(xml)
        {
            Modifiers = new GroupModifierNode(() => XmlBackend.Modifiers, this) {Controller = XmlBackend.Controller};
        }

        public override ICatalogueContext Context
        {
            get { return _context; }
            set
            {
                var old = _context;
                if (!Set(ref _context, value))
                    return;
                old?.GroupLinks.Deregister(this);
                Target = null;
                if (value != null)
                {
                    value.GroupLinks.Register(this);
                    value.Groups.SetTargetOf(this);
                }
                Modifiers.ChangeContext(value);
            }
        }

        public override INodeSimple<IGroupModifier> Modifiers { get; }
    }
}
