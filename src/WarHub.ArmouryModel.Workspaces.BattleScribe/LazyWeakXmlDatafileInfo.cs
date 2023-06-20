using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    internal sealed class LazyWeakXmlDatafileInfo : IDatafileInfo
    {
        public LazyWeakXmlDatafileInfo(string path, SourceKind dataKind)
        {
            Filepath = path;
            DataKind = dataKind;
        }

        public string Filepath { get; }

        public SourceKind DataKind { get; }

        private WeakReference<SourceNode?> WeakData { get; } = new WeakReference<SourceNode?>(null);

        public SourceNode? GetData(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<SourceNode?> GetDataAsync(CancellationToken cancellationToken = default)
        {
            if (WeakData.TryGetTarget(out var cached))
            {
                return Task.FromResult<SourceNode?>(cached);
            }
            var data = ReadFile(cancellationToken);
            WeakData.SetTarget(data);
            return Task.FromResult(data);
        }

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);

        public bool TryGetData(out SourceNode? node) => WeakData.TryGetTarget(out node);

        private SourceNode? ReadFile(CancellationToken cancellationToken = default)
        {
            try
            {
                using var filestream = File.OpenRead(Filepath);
                var data = filestream.LoadSourceAuto(Filepath, cancellationToken);
                return data;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to read file {Filepath}", e);
            }
        }
    }
}
