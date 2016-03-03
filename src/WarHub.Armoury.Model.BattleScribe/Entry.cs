// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using Nodes;

    public class Entry : EntryBase<BattleScribeXml.Entry>, IEntry
    {
        private readonly BookIndex _book;
        private readonly EntryModifierNode _modifiers;
        private readonly ProfileLinkNode _profileLinks;
        private readonly ProfileNode _profiles;
        private readonly RuleLinkNode _ruleLinks;
        private readonly RuleNode _rules;
        private ICatalogueContext _context;

        public Entry(BattleScribeXml.Entry xml)
            : base(xml)
        {
            _book = new BookIndex(XmlBackend);
            _modifiers = new EntryModifierNode(() => XmlBackend.Modifiers, this) {Controller = XmlBackend.Controller};
            _profileLinks = new ProfileLinkNode(() => XmlBackend.Links.ProfileLinks, this)
            {
                Controller = XmlBackend.Controller
            };
            _profiles = new ProfileNode(() => XmlBackend.Profiles, this) {Controller = XmlBackend.Controller};
            _ruleLinks = new RuleLinkNode(() => XmlBackend.Links.RuleLinks, this) {Controller = XmlBackend.Controller};
            _rules = new RuleNode(() => XmlBackend.Rules, this) {Controller = XmlBackend.Controller};
        }

        public IBookIndex Book => _book;

        public override ICatalogueContext Context
        {
            get { return _context; }
            set
            {
                var old = _context;
                if (!Set(ref _context, value))
                {
                    return;
                }
                old?.Entries.Deregister(this);
                value?.Entries.Register(this);
                Modifiers.ChangeContext(value);
                Profiles.ChangeContext(value);
                ProfileLinks.ChangeContext(value);
                Rules.ChangeContext(value);
                RuleLinks.ChangeContext(value);
                Groups.ChangeContext(value);
                GroupLinks.ChangeContext(value);
                Entries.ChangeContext(value);
                EntryLinks.ChangeContext(value);
            }
        }

        public INodeSimple<IEntryModifier> Modifiers => _modifiers;

        public decimal PointCost
        {
            get { return XmlBackend.Points; }
            set { Set(XmlBackend.Points, value, () => { XmlBackend.Points = value; }); }
        }

        public INode<IProfileLink, IProfile> ProfileLinks => _profileLinks;

        public INodeSimple<IProfile> Profiles => _profiles;

        public INode<IRuleLink, IRule> RuleLinks => _ruleLinks;

        public INodeSimple<IRule> Rules => _rules;

        public EntryType Type
        {
            get { return XmlBackend.Type; }
            set { Set(XmlBackend.Type, value, () => { XmlBackend.Type = value; }); }
        }

        public IEntry Clone()
        {
            return new Entry(new BattleScribeXml.Entry(XmlBackend));
        }
    }
}
