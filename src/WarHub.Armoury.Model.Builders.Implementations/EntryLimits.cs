namespace WarHub.Armoury.Model.Builders.Implementations
{
    using Mvvm;

    internal class EntryLimits : NotifyPropertyChangedBase, IEntryLimits
    {
        public IMinMax<int> InForceLimit { get; } = new MinMax<int>();
        public IMinMax<int> InRosterLimit { get; } = new MinMax<int>();
        public IMinMax<decimal> PointsLimit { get; } = new MinMax<decimal>();
        public IMinMax<int> SelectionsLimit { get; } = new MinMax<int>();
    }
}
