using System;
using System.IO;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    internal class LazyWeakXmlDatafileInfo<TData> : IDatafileInfo<TData> where TData : SourceNode
    {
        public LazyWeakXmlDatafileInfo(string path, SourceKind dataKind)
        {
            Filepath = path;
            DataKind = dataKind;
        }

        public string Filepath { get; }

        public TData Data => GetData();

        public SourceKind DataKind { get; }

        private WeakReference<TData> WeakData { get; } = new WeakReference<TData>(null);

        public TData GetData()
        {
            if (WeakData.TryGetTarget(out var cached))
            {
                return cached;
            }
            var data = ReadFile();
            WeakData.SetTarget(data);
            return data;
        }

        public string GetStorageName() => Path.GetFileNameWithoutExtension(Filepath);

        SourceNode IDatafileInfo.GetData() => GetData();

        private TData ReadFile()
        {
            using (var filestream = File.OpenRead(Filepath))
            {
                var datafile = filestream.LoadSourceAuto(Filepath);
                return (TData)datafile?.GetData();
            }
        }
    }
}
