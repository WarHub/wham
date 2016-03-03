// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System.Diagnostics;
    using Nodes;

    [DebuggerDisplay("{Name}, {Id.Value}")]
    public class Catalogue : CatalogueBase<BattleScribeXml.Catalogue>, ICatalogue
    {
        private readonly RootEntryNode _entries;
        private readonly RootLinkNode _entryLinks;
        private readonly IdLink<IGameSystem> _gameSystemLink;
        private readonly RuleLinkNode _ruleLinks;
        private readonly RuleNode _rules;
        private readonly EntryNode _sharedEntries;
        private readonly GroupNode _sharedGroups;
        private readonly ProfileNode _sharedProfiles;
        private readonly RuleNode _sharedRules;
        private ICatalogueContext _context;
        private IGameSystemContext _systemContext;

        public Catalogue(BattleScribeXml.Catalogue xml)
            : base(xml)
        {
            _gameSystemLink = new IdLink<IGameSystem>(
                XmlBackend.GameSystemGuid,
                newGuid => { XmlBackend.GameSystemGuid = newGuid; },
                () => XmlBackend.GameSystemId);
            _entries = new RootEntryNode(() => XmlBackend.Entries, this) {Controller = XmlBackend.Controller};
            _entryLinks = new RootLinkNode(() => XmlBackend.Links.EntryLinks, this) {Controller = XmlBackend.Controller};
            _ruleLinks = new RuleLinkNode(() => XmlBackend.Links.RuleLinks, this) {Controller = XmlBackend.Controller};
            _rules = new RuleNode(() => XmlBackend.Rules, this) {Controller = XmlBackend.Controller};
            _sharedEntries = new EntryNode(() => XmlBackend.SharedEntries, this) {Controller = XmlBackend.Controller};
            _sharedGroups = new GroupNode(() => XmlBackend.SharedEntryGroups, this) {Controller = XmlBackend.Controller};
            _sharedProfiles = new ProfileNode(() => XmlBackend.SharedProfiles, this)
            {
                Controller = XmlBackend.Controller
            };
            _sharedRules = new RuleNode(() => XmlBackend.SharedRules, this) {Controller = XmlBackend.Controller};
        }

        public ICatalogueContext Context
        {
            get { return _context; }
            private set
            {
                var old = _context;
                if (Set(ref _context, value))
                {
                    SharedRules.ChangeContext(_context);
                    SharedProfiles.ChangeContext(_context);
                    SharedGroups.ChangeContext(_context);
                    SharedEntries.ChangeContext(_context);
                    Rules.ChangeContext(_context);
                    RuleLinks.ChangeContext(_context);
                    Entries.ChangeContext(_context);
                    EntryLinks.ChangeContext(_context);
                }
            }
        }

        public INodeSimple<IRootEntry> Entries
        {
            get { return _entries; }
        }

        public INode<IRootLink, IEntry> EntryLinks
        {
            get { return _entryLinks; }
        }

        public IIdLink<IGameSystem> GameSystemLink
        {
            get { return _gameSystemLink; }
        }

        public INode<IRuleLink, IRule> RuleLinks
        {
            get { return _ruleLinks; }
        }

        public INodeSimple<IRule> Rules
        {
            get { return _rules; }
        }

        public INodeSimple<IEntry> SharedEntries
        {
            get { return _sharedEntries; }
        }

        public INodeSimple<IGroup> SharedGroups
        {
            get { return _sharedGroups; }
        }

        public INodeSimple<IProfile> SharedProfiles
        {
            get { return _sharedProfiles; }
        }

        public INodeSimple<IRule> SharedRules
        {
            get { return _sharedRules; }
        }

        public IGameSystemContext SystemContext
        {
            get { return _systemContext; }
            set
            {
                var old = _systemContext;
                if (!Set(ref _systemContext, value))
                {
                    return;
                }
                if (old != null)
                {
                    old.Catalogues.Deregister(this);
                }
                if (value != null)
                {
                    GameSystemLink.Target = value.GameSystem;
                    Context = new CatalogueContext(this);
                    value.Catalogues.Register(this);
                }
                else
                {
                    Context = null;
                    GameSystemLink.Target = null;
                }
            }
        }
    }
}
