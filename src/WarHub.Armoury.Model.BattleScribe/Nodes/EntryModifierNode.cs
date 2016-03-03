namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;

    internal class EntryModifierNode
        : XmlBackedNodeSimple<IEntryModifier, EntryModifier, Modifier, ICatalogueItem>
    {
        public EntryModifierNode(Func<IList<Modifier>> listGet, ICatalogueItem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IEntryModifier item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IEntryModifier item)
        {
            item.Context = null;
        }

        private static EntryModifier Factory(ICatalogueItem parent)
        {
            var modifier = new Modifier
            {
                Field = default(EntryField).XmlName(),
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

        private static EntryModifier Transformation(Modifier arg, ICatalogueItem parent)
        {
            return new EntryModifier(arg);
        }
    }
}
