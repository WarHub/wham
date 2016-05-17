// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System.Collections.Generic;

    public interface IRemoteDataSettings
    {
        IEnumerable<RemoteDataSourceInfo> Entries { get; }

        void AddEntry(RemoteDataSourceInfo entry);

        void RemoveEntry(RemoteDataSourceInfo entry);
    }
}
