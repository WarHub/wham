namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BattleScribeXml;

    internal class ProfileModifierNode
        : XmlBackedNodeSimple<IProfileModifier, ProfileModifier, Modifier, ICatalogueItem>
    {
        public ProfileModifierNode(Func<IList<Modifier>> listGet, ICatalogueItem parent)
            : base(parent, listGet, Transformation, Factory)
        {
        }

        protected override void ProcessItemAddition(IProfileModifier item)
        {
            item.Context = Parent.Context;
        }

        protected override void ProcessItemRemoval(IProfileModifier item)
        {
            item.Context = null;
        }

        private static ProfileModifier Factory(ICatalogueItem parent)
        {
            var profile = parent as IProfile ?? ((IProfileLink) parent).Target;
            var firstCharacteristicId = profile.Characteristics.First().TypeId;
            var modifier = new Modifier
            {
                Field = firstCharacteristicId.RawValue,
                FieldCharacteristicGuid = firstCharacteristicId.Value,
                NumberOfRepeats = 1,
                RepeatChildGuid = ReservedIdentifiers.NoChildId,
                RepeatChildId = ReservedIdentifiers.NoChildName,
                RepeatParentGuid = ReservedIdentifiers.RosterAncestorId,
                RepeatParentId = ReservedIdentifiers.RosterAncestorName,
                RepeatField = ConditionValueUnit.Selections,
                Repeating = false,
                RepeatValue = 1m,
                Type = default(ProfileModifierAction).XmlName(),
                Value = 0m.ToString()
            };
            return Transformation(modifier, parent);
        }

        private static ProfileModifier Transformation(Modifier arg, ICatalogueItem parent)
        {
            return new ProfileModifier(arg);
        }
    }
}
