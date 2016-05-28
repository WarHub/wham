// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.Serialization
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Provides methods to save and load abstract item or index.
    /// </summary>
    public interface IItemStore<TSerializableItem>
    {
        /// <summary>
        ///     Retrieves item from storage.
        /// </summary>
        /// <returns>Loaded item.</returns>
        Task<TSerializableItem> LoadItemAsync();

        /// <summary>
        ///     Stores item into storage.
        /// </summary>
        /// <param name="item">item to be saved.</param>
        Task SaveItemAsync(TSerializableItem item);
    }
}
