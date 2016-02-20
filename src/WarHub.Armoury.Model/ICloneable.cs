// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface ICloneable<T>
    {
        /// <summary>
        ///     Clones object but assigns it new Guid (if exists).
        /// </summary>
        /// <returns>Deep copy of object (with new Guid).</returns>
        T Clone();
    }
}
