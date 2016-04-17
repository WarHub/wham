// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders.Implementations
{
    public class CharacteristicBuilder : ICharacteristicBuilder
    {
        public CharacteristicBuilder(ICharacteristicType characteristicType, ICharacteristic characteristic)
        {
            CharacteristicType = characteristicType;
            Characteristic = characteristic;
        }

        public ICharacteristicType CharacteristicType { get; }

        public ICharacteristic Characteristic { get; }

        public string Value { get; set; }

        public void Reset()
        {
            Value = Characteristic.Value;
        }
    }
}
