// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;

    public class CharacteristicType : IdentifiedNamedModelBase<BattleScribeXml.CharacteristicType>,
        ICharacteristicType
    {
        public CharacteristicType(BattleScribeXml.CharacteristicType xml)
            : base(xml)
        {
        }
    }
}
