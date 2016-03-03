// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using System.Linq;
    using BattleScribeXml;
    using Nodes;

    public class Group : EntryBase<EntryGroup>, IGroup
    {
        private readonly GroupModifierNode _modifiers;
        private ICatalogueContext _context;

        public Group(EntryGroup xml)
            : base(xml)
        {
            _modifiers = new GroupModifierNode(() => XmlBackend.Modifiers, this) {Controller = XmlBackend.Controller};
        }

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
                old?.Groups.Deregister(this);
                value?.Groups.Register(this);
                Modifiers.ChangeContext(value);
                Groups.ChangeContext(value);
                GroupLinks.ChangeContext(value);
                Entries.ChangeContext(value);
                EntryLinks.ChangeContext(value);
            }
        }

        public IEntry DefaultChoice
        {
            get
            {
                return this.GetEntryLinkPairs()
                    .FirstOrDefault(pair => (pair.Link?.Id.Value ?? pair.Entry.Id.Value) == XmlBackend.DefaultEntryGuid)
                    ?
                    .Entry;
            }
            set
            {
                if (value != null && this.GetEntryLinkPairs().All(pair => !pair.Entry.IdValueEquals(value.Id.Value)))
                {
                    throw new ArgumentException($"{nameof(DefaultChoice)} must be null or group's direct child.",
                        nameof(value));
                }
                XmlBackend.DefaultEntryGuid = value?.Id.Value ?? Guid.Empty;
                RaiseCallingPropertyChanged();
            }
        }

        public INodeSimple<IGroupModifier> Modifiers => _modifiers;

        public IGroup Clone()
        {
            return new Group(new EntryGroup(XmlBackend));
        }
    }
}
