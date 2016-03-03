// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class CatalogueConditionNode
        : XmlBackedNodeSimple<ICatalogueCondition, CatalogueCondition, Condition,
            ICatalogueItem>
    {
        public CatalogueConditionNode(Func<IList<Condition>> listGet, ICatalogueItem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(ICatalogueCondition item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(ICatalogueCondition item)
        {
            item.Context = null;
        }

        private static CatalogueCondition Factory()
        {
            var condition = new Condition
            {
                ParentGuid = ReservedIdentifiers.RosterAncestorId,
                ParentId = ReservedIdentifiers.RosterAncestorName,
                Type = ConditionKind.EqualTo,
                Field = ConditionValueUnit.TotalSelections,
                ChildId = ReservedIdentifiers.NoChildName,
                ChildGuid = ReservedIdentifiers.NoChildId
            };
            return Transformation(condition);
        }

        private static CatalogueCondition Transformation(Condition arg)
        {
            return new CatalogueCondition(arg);
        }
    }
}
