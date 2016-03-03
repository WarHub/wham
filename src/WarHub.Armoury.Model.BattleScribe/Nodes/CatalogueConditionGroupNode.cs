// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class CatalogueConditionGroupNode
        : XmlBackedNodeSimple<ICatalogueConditionGroup, CatalogueConditionGroup,
            ConditionGroup, ICatalogueItem>
    {
        public CatalogueConditionGroupNode(Func<IList<ConditionGroup>> listGet,
            ICatalogueItem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(ICatalogueConditionGroup item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(ICatalogueConditionGroup item)
        {
            item.Context = null;
        }

        private static CatalogueConditionGroup Factory()
        {
            var condition = new ConditionGroup();
            return Transformation(condition);
        }

        private static CatalogueConditionGroup Transformation(ConditionGroup arg)
        {
            return new CatalogueConditionGroup(arg);
        }
    }
}
