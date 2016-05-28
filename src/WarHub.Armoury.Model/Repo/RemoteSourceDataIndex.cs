// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;
    using System.Collections.Generic;
    using Internal;

    /// <summary>
    ///     Describes contents of remote data source, and it's properties.
    /// </summary>
    public class RemoteSourceDataIndex : NotifyPropertyChangedBase, INameable, IProgramVersioned
    {
        private string _name;
        private string _originProgramVersion;
        private Uri _indexUri;

        public RemoteSourceDataIndex()
        {
        }

        public RemoteSourceDataIndex(IEnumerable<RemoteDataInfo> dataInfos)
        {
            RemoteDataInfos = new ObservableList<RemoteDataInfo>(dataInfos);
        }

        public Uri IndexUri
        {
            get { return _indexUri; }
            set { Set(ref _indexUri, value); }
        }

        public IObservableList<RemoteDataInfo> RemoteDataInfos { get; }
            = new ObservableList<RemoteDataInfo>();


        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        public string OriginProgramVersion
        {
            get { return _originProgramVersion; }
            set { Set(ref _originProgramVersion, value); }
        }
    }
}
