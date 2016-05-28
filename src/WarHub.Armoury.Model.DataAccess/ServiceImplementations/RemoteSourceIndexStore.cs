// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.DataAccess.ServiceImplementations
{
    using PCLStorage;
    using Serialization;

    public class RemoteSourceIndexStore : ItemStore<SerializableRemoteSourceIndex>, IRemoteSourceIndexStore
    {
        public RemoteSourceIndexStore(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        protected override string FileName { get; } = "RemoteSourceIndex.xml";
    }
}
