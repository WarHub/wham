// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Linq;

    /// <summary>
    ///     Extensions for <see cref="IGroup" /> to check various preset predicates.
    /// </summary>
    public static class GroupExtensions
    {
        /// <summary>
        ///     Checks whether this group is min/max 0/1 or 1/1 and has no subgroups.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static bool IsRadioGroup(this IGroup group)
        {
            var selLimits = group.Limits.SelectionsLimit;
            return !group.GetSubGroups().Any() && (selLimits.HasValues(0, 1) || selLimits.HasValues(1, 1));
        }
    }
}
