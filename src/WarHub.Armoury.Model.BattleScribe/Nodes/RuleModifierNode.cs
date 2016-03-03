// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class RuleModifierNode
        : XmlBackedNodeSimple<IRuleModifier, RuleModifier, Modifier, ICatalogueItem>
    {
        public RuleModifierNode(Func<IList<Modifier>> listGet, ICatalogueItem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IRuleModifier item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IRuleModifier item)
        {
            item.Context = null;
        }

        private static RuleModifier Factory(ICatalogueItem parent)
        {
            var modifier = new Modifier
            {
                Field = default(RuleField).XmlName(),
                NumberOfRepeats = 1,
                RepeatChildGuid = ReservedIdentifiers.NoChildId,
                RepeatChildId = ReservedIdentifiers.NoChildName,
                RepeatParentGuid = ReservedIdentifiers.RosterAncestorId,
                RepeatParentId = ReservedIdentifiers.RosterAncestorName,
                RepeatField = default(ConditionValueUnit),
                Repeating = false,
                RepeatValue = 1m,
                Type = default(RuleModifierAction).XmlName(),
                Value = 0m.ToString()
            };
            return Transformation(modifier, parent);
        }

        private static RuleModifier Transformation(Modifier arg, ICatalogueItem parent)
        {
            return new RuleModifier(arg);
        }
    }
}
