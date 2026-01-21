using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectModel
{
    public sealed class LazyDatafileSourceTree : SourceTree
    {
        private SourceNode? lazyRoot;

        public LazyDatafileSourceTree(IDatafileInfo datafile)
        {
            Datafile = datafile;
        }

        public IDatafileInfo Datafile { get; }

        public override string? FilePath => Datafile.Filepath;

        public override FileLinePositionSpan GetLineSpan(TextSpan span)
        {
            return default;
        }

        public override Location GetLocation(TextSpan span)
        {
            return Location.Create(this, span);
        }

        public override bool TryGetRoot([NotNullWhen(true)] out SourceNode? root)
        {
            if (Datafile.TryGetData(out var data))
            {
                root = NodeForThisTree(data);
                return true;
            }
            root = null;
            return false;
        }

        public override SourceNode GetRoot(CancellationToken cancellationToken = default)
        {
            try
            {
                var data = Datafile.GetData(cancellationToken);
                return GetAssociatedInterlockedNode(data);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
            {
                return ThrowNoData(ex);
            }
        }

        public override async Task<SourceNode> GetRootAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await Datafile.GetDataAsync(cancellationToken);
                return GetAssociatedInterlockedNode(data);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
            {
                return ThrowNoData(ex);
            }
        }

        private SourceNode GetAssociatedInterlockedNode(SourceNode data)
        {
            var result = NodeForThisTree(data ?? ThrowNoData());
            var previous = Interlocked.CompareExchange(ref lazyRoot, result, null);
            return previous is null ? result : previous;
        }

        public override SourceTree WithRoot(SourceNode root)
        {
            return CreateForRoot(root, FilePath);
        }

        private SourceNode ThrowNoData(Exception? inner = null)
        {
            throw new InvalidOperationException($"Failed to retrieve data from datafile with path '{FilePath}'.", inner);
        }
    }
}
