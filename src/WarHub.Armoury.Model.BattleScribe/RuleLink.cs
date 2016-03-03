// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using Nodes;

    public class RuleLink : ModifiableLink<IRule, IRuleModifier>, IRuleLink
    {
        private ICatalogueContext _context;

        public RuleLink(BattleScribeXml.Link xml)
            : base(xml)
        {
            Modifiers = new RuleModifierNode(() => XmlBackend.Modifiers, this) {Controller = XmlBackend.Controller};
        }

        public override ICatalogueContext Context
        {
            get { return _context; }
            set
            {
                var old = _context;
                if (!Set(ref _context, value))
                    return;
                old?.RuleLinks.Deregister(this);
                Target = null;
                value?.RuleLinks.Register(this);
                value?.Rules.SetTargetOf(this);
                Modifiers.ChangeContext(value);
            }
        }

        public override INodeSimple<IRuleModifier> Modifiers { get; }
    }
}
