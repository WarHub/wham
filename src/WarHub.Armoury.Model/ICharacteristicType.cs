// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    /// <summary>
    ///     Defines a Name and Id for a single type of characteristic under a single Profile Type.
    /// </summary>
    public interface ICharacteristicType : INameable
    {
        /// <summary>
        ///     Non-unique id, should be unique inside profile type.
        /// </summary>
        IIdentifier Id { get; }
    }
}
