using System.IO;
using System.Threading.Tasks;
using WarHub.Armoury.Model.Repo;

namespace WarHub.Armoury.Model.BattleScribe.Files
{
    public interface IBattleScribeFileHandler
    {
        Task<CatalogueInfo> MoveCatalogueToRepoStorageAsync(Stream stream, string filename);
        Task<GameSystemInfo> MoveGameSystemToRepoStorageAsync(Stream stream, string filename);
        Task<RosterInfo> MoveRosterToRepoStorageAsync(Stream stream, string filename);
        RemoteSourceDataIndex ReadIndexAuto(Stream stream, string filepath);
    }
}