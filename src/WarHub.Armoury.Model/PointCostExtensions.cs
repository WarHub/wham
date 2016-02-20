// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Linq;

    public static class PointCostExtensions
    {
        /// <summary>
        ///     Calculates point cost of entry's minimum selection cost (which in case min limit is 0,
        ///     totals to 0).
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static decimal GetMinPoints(this IEntry entry)
        {
            var min = entry.Limits.SelectionsLimit.Min;
            return min < 1 ? 0m : (entry.PointCost + entry.GetChildrenPoints())*min;
        }

        public static decimal GetTotalPoints(this ISelection selection)
        {
            return selection.Selections.Aggregate(selection.PointCost, (sum, x) => sum + x.GetTotalPoints());
        }

        public static decimal GetTotalPoints(this ICategoryMock categoryMock)
        {
            return categoryMock.Selections.Aggregate(0m, (sum, x) => sum + x.GetTotalPoints());
        }

        public static decimal GetTotalPoints(this IForce force)
        {
            return force.CategoryMocks.Aggregate(0m, (sum, x) => sum + GetTotalPoints(x))
                   + force.Forces.Aggregate(0m, (sum, x) => sum + x.GetTotalPoints());
        }

        public static decimal GetTotalPoints(this IRoster roster)
        {
            return roster.Forces.Aggregate(0m, (sum, x) => sum + x.GetTotalPoints());
        }

        /// <summary>
        ///     Assuming taking exactly one selection of entry, calculates point cost taking into
        ///     account required subentries.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static decimal GetTotalPoints(this IEntry entry)
        {
            return entry.PointCost + entry.GetChildrenPoints();
        }

        private static decimal GetChildrenPoints(this IEntry entry)
        {
            ;
            return entry.GetSubEntriesTotalPoints() + entry.GetSubGroupsTotalPoints();
        }

        private static decimal GetGroupDefaultPoints(this IGroup group)
        {
            var groupMin = group.Limits.SelectionsLimit.Min;
            var defaultChoice = group.DefaultChoice;
            if (defaultChoice == null)
                return 0;
            var selectionMin = defaultChoice.Limits.SelectionsLimit.Min;
            var selectionPoints = defaultChoice.GetTotalPoints();
            var subgroupsPoints = group.GetSubGroupsTotalPoints();
            return subgroupsPoints + selectionPoints*(selectionMin > groupMin ? selectionMin : groupMin);
        }

        private static decimal GetSubEntriesTotalPoints(this IEntriesLinkedNodeContainer parent)
        {
            return parent.Entries.Aggregate(0m, (sum, x) => sum + x.GetMinPoints())
                   + parent.EntryLinks.Aggregate(0m, (sum, x) => sum + x.Target.GetMinPoints());
        }

        private static decimal GetSubGroupsTotalPoints(this IGroupsLinkedNodeContainer parent)
        {
            return parent.Groups.Aggregate(0m, (sum, x) => sum + x.GetGroupDefaultPoints())
                   + parent.GroupLinks.Aggregate(0m, (sum, x) => sum + x.Target.GetGroupDefaultPoints());
        }
    }
}
