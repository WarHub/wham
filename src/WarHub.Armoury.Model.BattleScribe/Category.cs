// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;
    using Nodes;

    public class Category : IdentifiedNamedModelBase<BattleScribeXml.Category>, ICategory
    {
        private readonly Limits<bool, bool, bool> _isAddedToParent;
        private readonly Limits<int, decimal, int> _limits;
        private readonly CategoryModifierNode _modifiers;
        private IGameSystemContext _context;

        public Category(BattleScribeXml.Category xml)
            : base(xml)
        {
            _isAddedToParent = LimitsFactory.CreateIsAddedToParent(xml);
            _limits = LimitsFactory.CreateLimits(xml);
            _modifiers = new CategoryModifierNode(() => XmlBackend.Modifiers, this) {Controller = XmlBackend.Controller};
        }

        public INodeSimple<ICategoryModifier> CategoryModifiers
        {
            get { return _modifiers; }
        }

        public IGameSystemContext Context
        {
            get { return _context; }
            set
            {
                var old = _context;
                if (!Set(ref _context, value))
                {
                    return;
                }
                if (old != null && old.Categories.IsRegistered(this))
                {
                    old.Categories.Deregister(this);
                }
                if (value != null && !value.Categories.IsRegistered(this))
                {
                    value.Categories.Register(this);
                }
                CategoryModifiers.ChangeContext(value);
            }
        }

        public ILimits<bool, bool, bool> IsAddedToParent
        {
            get { return _isAddedToParent; }
        }

        public ILimits<int, decimal, int> Limits
        {
            get { return _limits; }
        }
    }
}
