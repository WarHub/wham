// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using BattleScribe.Services;
    using Repo;
    using Xunit;

    public class CatalogueTests
    {
        public CatalogueTests()
        {
            CreateManager();
        }

        private BattleScribeRepoManager Manager { get; set; }

        public void CreateManager()
        {
            Manager = new BattleScribeRepoManager(new SystemIndex(), new SpaceMarineStorageService());
        }

        [Fact]
        public async Task LoadSpaceMarineCatalogueTest()
        {
            var marineCatalogue = await Manager.GetCatalogueAsync(Manager.SystemIndex.CatalogueInfos.First());
            Assert.NotNull(marineCatalogue);
        }

        private class SpaceMarineStorageService : IRepoStorageService
        {
            public Task DeleteCatalogueAsync(CatalogueInfo info) => Throw<Task>();

            public Task DeleteGameSystemAsync(GameSystemInfo info) => Throw<Task>();

            public Task DeleteRosterAsync(RosterInfo info) => Throw<Task>();

            public Task<Stream> GetCatalogueOutputStreamAsync(CatalogueInfo info, string filename = null)
                => Throw<Task<Stream>>();

            public Task<Stream> GetGameSystemOutputStreamAsync(GameSystemInfo info, string filename = null)
                => Throw<Task<Stream>>();

            public Task<Stream> GetRosterOutputStreamAsync(RosterInfo info, string filename = null)
                => Throw<Task<Stream>>();

            public async Task<ICatalogue> LoadCatalogueAsync(CatalogueInfo catalogueInfo,
                LoadStreamedCatalogueCallback loadCatalogueFromStream)
            {
                using (var stream = File.OpenRead(Path.Combine(TestData.InputDir, TestData.CatalogueFilename)))
                {
                    var catalogue = loadCatalogueFromStream(stream);
                    return await Task.FromResult(catalogue);
                }
            }

            public async Task<IGameSystem> LoadGameSystemAsync(GameSystemInfo gameSystemInfo,
                LoadStreamedGameSystemCallback loadGameSystemFromStream)
            {
                using (var stream = File.OpenRead(Path.Combine(TestData.InputDir, TestData.GameSystemFilename)))
                {
                    var gameSystem = loadGameSystemFromStream(stream);
                    return await Task.FromResult(gameSystem);
                }
            }

            public Task<IRoster> LoadRosterAsync(RosterInfo info, LoadStreamedRosterAsyncCallback loadAsyncCallback)
                => Throw<Task<IRoster>>();

            public Task<IRoster> LoadRosterAsync(RosterInfo info, LoadStreamedRosterCallback loadCallback)
                => Throw<Task<IRoster>>();

#pragma warning disable 67
            public event EventHandler<NotifyRepoChangedEventArgs> RepoChanged;
#pragma warning restore 67

            private static T Throw<T>()
            {
                throw new NotSupportedException("This is a test implementation.");
            }
        }

        private class SystemIndex : ModelBase, ISystemIndex
        {
            public SystemIndex()
            {
                GameSystemRawId = "gstId";
                var catalogueInfo = new CatalogueInfo("name", "id", 0, "gstId", "v", "book", "author");
                CatalogueInfos = new ObservableList<CatalogueInfo>(new[] {catalogueInfo});
                GameSystemInfo = new GameSystemInfo("name", GameSystemRawId, 0, "v", "book", "author");
                RosterInfos = new ObservableList<RosterInfo>();
            }

            public IObservableReadonlySet<CatalogueInfo> CatalogueInfos { get; }

            public GameSystemInfo GameSystemInfo { get; }

            public string GameSystemRawId { get; }

            public IObservableReadonlySet<RosterInfo> RosterInfos { get; }
        }
    }
}
