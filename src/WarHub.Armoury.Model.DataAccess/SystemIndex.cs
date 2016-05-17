// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using System.Linq;
    using Internal;
    using Repo;
    using Serialization;

    public class SystemIndex : NotifyPropertyChangedBase, ISystemIndex
    {
        private GameSystemInfo _gameSystemInfo;

        /// <summary>
        ///     For serialization use.
        /// </summary>
        public SystemIndex(SerializableSystemIndex index)
        {
            GameSystemRawId = index.GameSystemRawId;
            _gameSystemInfo = index.SerializableGameSystemInfo;
            var catalogueInfos = index.CatalogueInfosSerializable.Select(x => (CatalogueInfo) x);
            CatalogueInfos = new ObservableList<CatalogueInfo>(catalogueInfos);
            var rosterInfos = index.RosterInfosSerializable.Select(x => (RosterInfo) x);
            RosterInfos = new ObservableList<RosterInfo>(rosterInfos);
        }

        public SystemIndex(string gameSystemRawId)
        {
            GameSystemRawId = gameSystemRawId;
            _gameSystemInfo = null;
            CatalogueInfos = new ObservableList<CatalogueInfo>();
            RosterInfos = new ObservableList<RosterInfo>();
        }

        public SystemIndex(GameSystemInfo gameSystemInfo)
            : this(gameSystemInfo.RawId)
        {
            _gameSystemInfo = gameSystemInfo;
        }

        public ObservableList<CatalogueInfo> CatalogueInfos { get; }

        public ObservableList<RosterInfo> RosterInfos { get; }

        public GameSystemInfo GameSystemInfo
        {
            get { return _gameSystemInfo; }
            set
            {
                _gameSystemInfo = value;
                RaisePropertyChanged();
            }
        }

        public string GameSystemRawId { get; }

        IObservableReadonlySet<CatalogueInfo> ISystemIndex.CatalogueInfos => CatalogueInfos;

        IObservableReadonlySet<RosterInfo> ISystemIndex.RosterInfos => RosterInfos;
    }
}
