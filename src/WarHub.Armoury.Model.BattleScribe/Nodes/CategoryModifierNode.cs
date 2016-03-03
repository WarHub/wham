// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class CategoryModifierNode
        : XmlBackedNodeSimple<ICategoryModifier, CategoryModifier, Modifier, ICategory>
    {
        public CategoryModifierNode(Func<IList<Modifier>> listGet, ICategory parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(ICategoryModifier item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(ICategoryModifier item)
        {
            item.Context = null;
        }

        private static CategoryModifier Factory(ICategory parent)
        {
            var modifier = new Modifier
            {
                Field = default(LimitField).XmlName(),
                NumberOfRepeats = 1,
                RepeatChildGuid = Guid.Empty,
                RepeatChildId = null,
                RepeatParentGuid = ReservedIdentifiers.RosterAncestorId,
                RepeatParentId = ReservedIdentifiers.RosterAncestorName,
                RepeatField = ConditionValueUnit.PointsLimit,
                Repeating = false,
                RepeatValue = 1m,
                Type = default(CategoryModifierAction).XmlName(),
                Value = 0m.ToString()
            };
            return Transformation(modifier, parent);
        }

        private static CategoryModifier Transformation(Modifier arg, ICategory parent)
        {
            return new CategoryModifier(arg);
        }
    }
}
