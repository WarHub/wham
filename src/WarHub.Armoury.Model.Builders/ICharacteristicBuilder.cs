// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Builders
{
    public interface ICharacteristicBuilder
    {
        ICharacteristic Characteristic { get; }

        ICharacteristicType CharacteristicType { get; }

        string Value { get; set; }

        void Reset();
    }
}
