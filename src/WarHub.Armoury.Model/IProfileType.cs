// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    /// <summary>
    ///     Defines a set of unique characteristics, each named and uniquely identified. Has an id and a name.
    /// </summary>
    public interface IProfileType : IIdentifiable, INameable, IGameSystemItem
    {
        INodeSimple<ICharacteristicType> CharacteristicTypes { get; }
    }
}
