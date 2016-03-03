// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

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
            xml.SetNewGuid();
            return Transformation(xml);
        }

        private static CharacteristicType Transformation(BattleScribeXml.CharacteristicType arg)
        {
            return new CharacteristicType(arg);
        }
    }
}
