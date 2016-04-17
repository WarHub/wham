namespace WarHub.Armoury.Model.Builders.Implementations
{
    public static class EntryLimitsCopyExtensions
    {
        public static void CopyFrom(this IEntryLimits @this, IEntryBase entryBase)
        {
            @this.CopyFrom(entryBase.Limits);
        }

        public static void CopyFrom(this IEntryLimits @this, IEntryLimits other)
        {
            @this.InForceLimit.CopyFrom(other.InForceLimit);
            @this.InRosterLimit.CopyFrom(other.InRosterLimit);
            @this.PointsLimit.CopyFrom(other.PointsLimit);
            @this.SelectionsLimit.CopyFrom(other.SelectionsLimit);
        }
    }
}
