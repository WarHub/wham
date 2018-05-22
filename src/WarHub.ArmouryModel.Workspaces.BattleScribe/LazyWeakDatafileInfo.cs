using System;
using System.IO;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public class LazyWeakDatafileInfo<TData> : IDatafileInfo<TData> where TData : SourceNode
    {
        public LazyWeakDatafileInfo(string path)
        {
            Filepath = path;
        }

        public string Filepath { get; }

        public TData Data => GetData();

        private WeakReference<TData> WeakData { get; } = new WeakReference<TData>(null);

        SourceNode IDatafileInfo.Data => Data;

        private TData GetData()
        {
            if (WeakData.TryGetTarget(out var cached))
            {
                return cached;
            }
            var data = ReadFile();
            WeakData.SetTarget(data);
            return data;
        }

        private TData ReadFile()
        {
            using (var filestream = File.OpenRead(Filepath))
            {
                var datafile = filestream.LoadSourceAuto(Filepath);
                return (TData)datafile?.Data;
            }
        }
    }
}
