// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using Nodes;

    public class EntryLink : ModifiableLink<IEntry, IEntryModifier>, IEntryLink
    {
        private ICatalogueContext _context;

        public EntryLink(BattleScribeXml.Link xml)
            : base(xml)
        {
            Modifiers = new EntryModifierNode(() => XmlBackend.Modifiers, this) {Controller = XmlBackend.Controller};
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
                old?.EntryLinks.Deregister(this);
                Target = null;
                if (value != null)
                {
                    value.EntryLinks.Register(this);
                    value.Entries.SetTargetOf(this);
                }
                Modifiers.ChangeContext(value);
            }
        }

        public override INodeSimple<IEntryModifier> Modifiers { get; }
    }
}
