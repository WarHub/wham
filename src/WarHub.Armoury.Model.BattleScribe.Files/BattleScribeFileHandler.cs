// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe.Files
{
    using System.IO;
    using System.Threading.Tasks;
    using Repo;
    using Services;

    public class BattleScribeFileHandler : IBattleScribeFileHandler
    {
        public BattleScribeFileHandler(IRepoStorageService repoStorageService)
        {
            RepoStorageService = repoStorageService;
        }

        private IRepoStorageService RepoStorageService { get; }

        public RemoteSourceDataIndex ReadIndexAuto(Stream stream, string filepath)
        {
            return DataIndexFile.ReadBattleScribeIndexAuto(filepath, stream);
        }

        public Task<CatalogueInfo> MoveCatalogueToRepoStorageAsync(Stream stream, string filename)
        {
            return CatalogueFile.MoveToRepoStorageAsync(stream, filename, RepoStorageService);
        }

        public Task<GameSystemInfo> MoveGameSystemToRepoStorageAsync(Stream stream, string filename)
        {
            return GameSystemFile.MoveToRepoStorageAsync(stream, filename, RepoStorageService);
        }

        public Task<RosterInfo> MoveRosterToRepoStorageAsync(Stream stream, string filename)
        {
            return RosterFile.MoveToRepoStorageAsync(stream, filename, RepoStorageService);
        }
    }
}
