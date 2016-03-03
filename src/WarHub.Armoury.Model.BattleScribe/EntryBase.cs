// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;
    using Nodes;

    public abstract class EntryBase<TXml> : IdentifiedNamedModelBase<TXml>, IEntryBase
        where TXml : BattleScribeXml.IEntryBase
    {
        private readonly EntryNode _entries;
        private readonly EntryLinkNode _entryLinks;
        private readonly GroupLinkNode _groupLinks;
        private readonly GroupNode _groups;
        private readonly EntryLimits _limits;

        protected EntryBase(TXml xml)
            : base(xml)
        {
            _entries = new EntryNode(() => XmlBackend.Entries, this);
            _entryLinks = new EntryLinkNode(() => XmlBackend.Links.EntryLinks, this);
            _groups = new GroupNode(() => XmlBackend.EntryGroups, this);
            _groupLinks = new GroupLinkNode(() => XmlBackend.Links.EntryGroupLinks, this);
            _limits = new EntryLimits(xml);
        }

        public abstract ICatalogueContext Context { get; set; }

        public INodeSimple<IEntry> Entries => _entries;

        public INode<IEntryLink, IEntry> EntryLinks => _entryLinks;

        public INode<IGroupLink, IGroup> GroupLinks => _groupLinks;

        public INodeSimple<IGroup> Groups => _groups;

        public bool IsCollective
        {
            get { return XmlBackend.Collective; }
            set { Set(XmlBackend.Collective, value, () => XmlBackend.Collective = value); }
        }

        public bool IsHidden
        {
            get { return XmlBackend.Hidden; }
            set { Set(XmlBackend.Hidden, value, () => XmlBackend.Hidden = value); }
        }

        public IEntryLimits Limits => _limits;
    }
}
