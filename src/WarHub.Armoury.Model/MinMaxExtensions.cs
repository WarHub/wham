// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;

    /// <summary>
    ///     Extensions for <see cref="IMinMax{T}" /> limits class.
    /// </summary>
    public static class MinMaxExtensions
    {
        public static bool MinEqualsMax(this IMinMax<int> limit) => limit.Min == limit.Max;

        public static bool MinEqualsMaxEquals(this IMinMax<int> limit, int value)
            => limit.Min == limit.Max && limit.Max == value;

        public static bool MinLesserThanMax(this IMinMax<int> limit) => limit.Max == -1 || limit.Min < limit.Max;
        public static bool MinLesserOrEqualsMax(this IMinMax<int> limit) => limit.Max == -1 || limit.Min <= limit.Max;

        public static bool MinAndMaxEquals(this IMinMax<int> limit, int other)
            => limit.MinEqualsMax() && limit.Min == other;

        /// <summary>
        ///     Checks if <paramref name="limit" />'s <see cref="IMinMax{T}.Min" /> is 0 and <see cref="IMinMax{T}.Max" /> is 1.
        /// </summary>
        /// <param name="limit">MinMax limit to check.</param>
        /// <returns>True if limit is binary.</returns>
        public static bool IsBinary(this IMinMax<int> limit) => limit.Min == 0 && limit.Max == 1;

        /// <summary>
        ///     Sets both min and max values in one method call. Convenience short for calling both property setters.
        /// </summary>
        /// <typeparam name="T">Type of limit.</typeparam>
        /// <param name="limit">Limit to set min max of.</param>
        /// <param name="min">Set as limit's min value.</param>
        /// <param name="max">Set as limit's max value.</param>
        public static void SetValues<T>(this IMinMax<T> limit, T min, T max)
        {
            limit.Min = min;
            limit.Max = max;
        }

        /// <summary>
        ///     Checks both min and max values in one method call. Convenience short for calling both property getters and
        ///     comparing them.
        /// </summary>
        /// <typeparam name="T">Type of limit.</typeparam>
        /// <param name="limit">Limit to check.</param>
        /// <param name="min">Compared with min value.</param>
        /// <param name="max">Compared with max value.</param>
        /// <returns>
        ///     True if <paramref name="min" /> == <paramref name="limit" />.<see cref="IMinMax{T}.Min" /> and
        ///     <paramref name="max" /> == <paramref name="limit" />.<see cref="IMinMax{T}.Max" />
        /// </returns>
        public static bool HasValues<T>(this IMinMax<T> limit, T min, T max)
        {
            if (limit == null)
                throw new ArgumentNullException(nameof(limit));
            return (limit.Min?.Equals(min) ?? min == null) && (limit.Max?.Equals(max) ?? max == null);
        }

        /// <summary>
        ///     Checks whether <paramref name="value" /> falls within range of the <paramref name="limit" />.
        /// </summary>
        /// <param name="limit">Limit to check against.</param>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if min &lt;= value &lt; max.</returns>
        public static bool ValueInRange(this IMinMax<int> limit, int value)
        {
            return value >= limit.Min && (limit.Max == -1 || value <= limit.Max);
        }
    }
}
