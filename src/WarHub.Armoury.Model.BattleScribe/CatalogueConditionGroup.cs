// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using Nodes;

    public class CatalogueConditionGroup : ConditionGroup, ICatalogueConditionGroup
    {
        private ICatalogueContext _context;

        public CatalogueConditionGroup(BattleScribeXml.ConditionGroup xml)
            : base(xml)
        {
            Conditions = new CatalogueConditionNode(() => XmlBackend.Conditions, this)
            {
                Controller = XmlBackend.Controller
            };
            ConditionGroups = new CatalogueConditionGroupNode(() => XmlBackend.ConditionGroups, this)
            {
                Controller = XmlBackend.Controller
            };
        }

        public INodeSimple<ICatalogueConditionGroup> ConditionGroups { get; }

        public INodeSimple<ICatalogueCondition> Conditions { get; }

        public ICatalogueContext Context
        {
            get { return _context; }
            set
            {
                if (!Set(ref _context, value))
                    return;
                Conditions.ChangeContext(value);
                ConditionGroups.ChangeContext(value);
            }
        }
    }
}
