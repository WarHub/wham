namespace WarHub.Armoury.Model.Builders.Implementations
{
    using Mvvm;

    internal class GeneralLimits : NotifyPropertyChangedBase, ILimits<int, decimal, int>
    {
        public IMinMax<int> PercentageLimit { get; } = new MinMax<int>();
        public IMinMax<decimal> PointsLimit { get; } = new MinMax<decimal>();
        public IMinMax<int> SelectionsLimit { get; } = new MinMax<int>();
    }
}
