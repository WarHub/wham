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
        public RemoteDataSourceIndex ReadIndexAuto(string filepath, Stream stream)
        {
            return DataIndexFile.ReadBattleScribeIndexAuto(filepath, stream);
        }

        public Task<CatalogueInfo> MoveCatalogueToRepoStorageAsync(Stream stream, string filename,
            IRepoStorageService repoStorageService)
        {
            return CatalogueFile.MoveToRepoStorageAsync(stream, filename, repoStorageService);
        }

        public Task<GameSystemInfo> MoveGameSystemToRepoStorageAsync(Stream stream, string filename,
            IRepoStorageService repoStorageService)
        {
            return GameSystemFile.MoveToRepoStorageAsync(stream, filename, repoStorageService);
        }

        public Task<RosterInfo> MoveRosterToRepoStorageAsync(Stream stream, string filename,
            IRepoStorageService repoStorageService)
        {
            return RosterFile.MoveToRepoStorageAsync(stream, filename, repoStorageService);
        }
    }
}
