// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess
{
    using Repo;

    /// <summary>
    ///     Describes single remote data source's name and <see cref="System.Uri" /> .
    /// </summary>
    public class RemoteDataSourceInfo
    {
        public RemoteDataSourceInfo(string name, string indexUri)
        {
            Name = name;
            IndexUri = indexUri;
        }

        public RemoteDataSourceInfo(RemoteDataSourceIndex index)
        {
            Name = index.Name;
            IndexUri = index.IndexUri.AbsoluteUri;
        }

        public string IndexUri { get; }

        public string Name { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;
            var other = obj as RemoteDataSourceInfo;
            if (other == null)
                return false;
            return IndexUri.Equals(other.IndexUri)
                   && Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 0;
                hash = hash*397 + Name.GetHashCode();
                hash = hash*397 + IndexUri.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"[{Name}]({IndexUri})";
        }
    }
}
