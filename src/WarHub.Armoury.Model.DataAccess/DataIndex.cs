// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System.Collections.Generic;
    using System.Linq;
    using Internal;
    using Repo;

    public class DataIndex : NotifyPropertyChangedBase
    {
        public DataIndex()
        {
            SystemIndexes = new ObservableList<ISystemIndex>();
        }

        public DataIndex(IEnumerable<ISystemIndex> collection)
        {
            SystemIndexes = new ObservableList<ISystemIndex>(collection);
        }

        public ISystemIndex this[GameSystemInfo gameSystemInfo]
        {
            get { return SystemIndexes.FirstOrDefault(x => x.GameSystemInfo.Equals(gameSystemInfo)); }
        }

        public ObservableList<ISystemIndex> SystemIndexes { get; }
    }
}
