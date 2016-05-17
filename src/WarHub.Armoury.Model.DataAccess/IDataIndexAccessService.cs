// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Provides methods to save and load index independent of platform.
    /// </summary>
    public interface IDataIndexAccessService
    {
        /// <summary>
        ///     Retrieves index from platform and user specific location.
        /// </summary>
        /// <returns>Null if operation fails.</returns>
        Task<DataIndex> LoadIndexAsync();

        /// <summary>
        ///     Stores index in platform and user specific location.
        /// </summary>
        /// <param name="index">Object to be stored.</param>
        Task SaveIndexAsync(DataIndex index);
    }
}
