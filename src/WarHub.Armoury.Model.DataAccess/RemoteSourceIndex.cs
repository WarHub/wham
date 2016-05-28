// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System.Collections.Generic;
    using Internal;
    using Repo;

    public class RemoteSourceIndex : NotifyPropertyChangedBase
    {
        public RemoteSourceIndex()
        {
            RemoteSources = new ObservableList<RemoteSource>();
        }

        public RemoteSourceIndex(IEnumerable<RemoteSource> collection)
        {
            RemoteSources = new ObservableList<RemoteSource>(collection);
        }

        public ObservableList<RemoteSource> RemoteSources { get; }
    }
}
