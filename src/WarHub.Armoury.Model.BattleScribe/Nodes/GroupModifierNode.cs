namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class GroupModifierNode
        : XmlBackedNodeSimple<IGroupModifier, GroupModifier, Modifier, ICatalogueItem>
    {
        public GroupModifierNode(Func<IList<Modifier>> listGet, ICatalogueItem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IGroupModifier item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IGroupModifier item)
        {
            item.Context = null;
        }

        private static GroupModifier Factory(ICatalogueItem parent)
        {
            var modifier = new Modifier
            {
                Field = default(GroupField).XmlName(),
                NumberOfRepeats = 1,
                RepeatChildGuid = ReservedIdentifiers.NoChildId,
                RepeatChildId = ReservedIdentifiers.NoChildName,
                RepeatParentGuid = ReservedIdentifiers.RosterAncestorId,
                RepeatParentId = ReservedIdentifiers.RosterAncestorName,
                RepeatField = ConditionValueUnit.Selections,
                Repeating = false,
                RepeatValue = 1m,
                Type = default(EntryBaseModifierAction).XmlName(),
                Value = 0m.ToString()
            };
            return Transformation(modifier, parent);
        }

        private static GroupModifier Transformation(Modifier arg, ICatalogueItem parent)
        {
            return new GroupModifier(arg);
        }
    }
}
