namespace WarHub.Armoury.Model.BattleScribe.Nodes
{
    using System;
    using System.Collections.Generic;
    using BattleScribeXml;
    using CharacteristicType = BattleScribe.CharacteristicType;

    internal class CharacteristicTypeNode
        : XmlBackedNodeSimple<ICharacteristicType, CharacteristicType, BattleScribeXml.CharacteristicType>
    {
        public CharacteristicTypeNode(Func<IList<BattleScribeXml.CharacteristicType>> listGet)
            : base(listGet, Transformation, Factory)
        {
        }

        private static CharacteristicType Factory()
        {
            var xml = new BattleScribeXml.CharacteristicType();
            IdentifiedExtensions.SetNewGuid(xml);
            return Transformation(xml);
        }

        private static CharacteristicType Transformation(BattleScribeXml.CharacteristicType arg)
        {
            return new CharacteristicType(arg);
        }
    }
}
