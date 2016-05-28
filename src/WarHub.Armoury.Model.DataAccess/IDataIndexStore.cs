// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using Serialization;

    /// <summary>
    ///     Provides methods to save and load index independent of platform.
    /// </summary>
    public interface IDataIndexStore : IItemStore<SerializableDataIndex>
    {
    }
}
