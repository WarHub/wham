// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.ServiceImplementations
{
    using PCLStorage;
    using Serialization;

    public class DataIndexStore : ItemStore<SerializableDataIndex>, IDataIndexStore
    {
        public DataIndexStore(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        protected override string FileName { get; } = "DataIndex.xml";
    }
}
