namespace WarHub.Armoury.Model.Builders.Implementations
{
    internal static class MinMaxCopyExtension
    {
        public static void CopyFrom<T>(this IMinMax<T> @this, IMinMax<T> other)
        {
            @this.Max = other.Max;
            @this.Min = other.Min;
        }
    }
}
