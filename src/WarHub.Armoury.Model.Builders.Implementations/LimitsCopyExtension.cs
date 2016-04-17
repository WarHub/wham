namespace WarHub.Armoury.Model.Builders.Implementations
{
    public static class LimitsCopyExtension
    {
        public static void CopyFrom(this ILimits<int, decimal, int> @this, ILimits<int, decimal, int> other)
        {
            @this.PercentageLimit.CopyFrom(other.PercentageLimit);
            @this.PointsLimit.CopyFrom(other.PointsLimit);
            @this.SelectionsLimit.CopyFrom(other.SelectionsLimit);
        }
    }
}
