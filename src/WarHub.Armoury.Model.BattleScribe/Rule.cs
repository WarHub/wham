// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;
    using Nodes;

    public class Rule : IdentifiedNamedIndexedModelBase<BattleScribeXml.Rule>, IRule
    {
        private readonly RuleModifierNode _modifiers;
        private ICatalogueContext _context;

        public Rule(BattleScribeXml.Rule xml)
            : base(xml)
        {
            _modifiers = new RuleModifierNode(() => XmlBackend.Modifiers, this) {Controller = XmlBackend.Controller};
        }

        public ICatalogueContext Context
        {
            get { return _context; }
            set
            {
                var old = _context;
                if (Set(ref _context, value))
                {
                    if (old != null)
                    {
                        old.Rules.Deregister(this);
                    }
                    if (value != null)
                    {
                        value.Rules.Register(this);
                    }
                    Modifiers.ChangeContext(value);
                }
            }
        }

        public string DescriptionText
        {
            get { return XmlBackend.Description; }
            set { Set(XmlBackend.Description, value, () => XmlBackend.Description = value); }
        }

        public bool IsHidden
        {
            get { return XmlBackend.Hidden; }
            set { Set(XmlBackend.Hidden, value, () => { XmlBackend.Hidden = value; }); }
        }

        public INodeSimple<IRuleModifier> Modifiers
        {
            get { return _modifiers; }
        }

        public IRule Clone()
        {
            return new Rule(new BattleScribeXml.Rule(XmlBackend));
        }
    }
}
