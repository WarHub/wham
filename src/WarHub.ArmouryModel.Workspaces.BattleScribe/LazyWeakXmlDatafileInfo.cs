using System;
using System.IO;
using System.Threading.Tasks;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    internal class LazyWeakXmlDatafileInfo : IDatafileInfo
    {
        public LazyWeakXmlDatafileInfo(string path, SourceKind dataKind)
        {
            Filepath = path;
            DataKind = dataKind;
        }

        public string Filepath { get; }

        public SourceKind DataKind { get; }

        private WeakReference<SourceNode?> WeakData { get; } = new WeakReference<SourceNode?>(null);

        public async Task<SourceNode?> GetDataAsync()
        {
            if (WeakData.TryGetTarget(out var cached))
            {
                return cached;
            }
            var data = await ReadFileAsync();
            WeakData.SetTarget(data);
            return data;
        }

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);

        private async Task<SourceNode?> ReadFileAsync()
        {
            using var filestream = File.OpenRead(Filepath);
            var datafile = filestream.LoadSourceAuto(Filepath);
            return datafile is null ? null : await datafile.GetDataAsync();
        }
    }
}
