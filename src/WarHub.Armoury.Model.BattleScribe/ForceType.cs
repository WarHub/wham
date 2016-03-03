// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;
    using Nodes;

    public class ForceType : IdentifiedNamedModelBase<BattleScribeXml.ForceType>, IForceType
    {
        private readonly CategoryNode _categories;
        private readonly ForceTypeNode _forceTypes;
        private readonly Limits<bool, bool, bool> _isAddedToParent;
        private readonly Limits<int, decimal, int> _limits;
        private IGameSystemContext _context;

        public ForceType(BattleScribeXml.ForceType xml)
            : base(xml)
        {
            _categories = new CategoryNode(() => XmlBackend.Categories, this) {Controller = XmlBackend.Controller};
            _forceTypes = new ForceTypeNode(() => XmlBackend.ForceTypes, this) {Controller = XmlBackend.Controller};
            _isAddedToParent = LimitsFactory.CreateIsAddedToParent(xml);
            _limits = LimitsFactory.CreateLimits(xml);
        }

        public INodeSimple<ICategory> Categories
        {
            get { return _categories; }
        }

        public IGameSystemContext Context
        {
            get { return _context; }
            set
            {
                var old = _context;
                if (Set(ref _context, value))
                {
                    if (old != null)
                    {
                        old.ForceTypes.Deregister(this);
                    }
                    Categories.ChangeContext(value);
                    ForceTypes.ChangeContext(value);
                    if (value != null)
                    {
                        value.ForceTypes.Register(this);
                    }
                }
            }
        }

        public INodeSimple<IForceType> ForceTypes
        {
            get { return _forceTypes; }
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
