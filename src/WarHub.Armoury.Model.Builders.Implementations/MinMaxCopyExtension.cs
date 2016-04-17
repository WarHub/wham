// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

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
