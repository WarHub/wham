// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    ///     Describes contents of remote data source, and it's properties.
    /// </summary>
    public class RemoteDataSourceIndex : INameable, IProgramVersioned, INotifyPropertyChanged
    {
        private string _name;
        private string _originProgramVersion;
        private Uri _sourceUri;

        public RemoteDataSourceIndex()
        {
        }

        public RemoteDataSourceIndex(IEnumerable<RemoteDataInfo> dataInfos)
        {
            RemoteDataInfos = new ObservableList<RemoteDataInfo>(dataInfos);
        }

        public Uri IndexUri
        {
            get { return _sourceUri; }
            set { Set(ref _sourceUri, value, nameof(IndexUri)); }
        }

        public IObservableList<RemoteDataInfo> RemoteDataInfos { get; }
            = new ObservableList<RemoteDataInfo>();

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value, nameof(Name)); }
        }

        public string OriginProgramVersion
        {
            get { return _originProgramVersion; }
            set { Set(ref _originProgramVersion, value, nameof(OriginProgramVersion)); }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Set<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }
            field = value;
            RaisePropertyChanged(propertyName);
        }
    }
}
