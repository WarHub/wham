// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;

    public class Limits<TSelectionLim, TPointsLim, TPercentLim>
        : ModelBase, ILimits<TSelectionLim, TPointsLim, TPercentLim>
    {
        public Limits(
            IMinMax<TSelectionLim> selectionsLimit,
            IMinMax<TPointsLim> pointsLimit,
            IMinMax<TPercentLim> percentageLimit)
        {
            SelectionsLimit = selectionsLimit;
            PointsLimit = pointsLimit;
            PercentageLimit = percentageLimit;
        }

        public IMinMax<TPercentLim> PercentageLimit { get; }

        public IMinMax<TPointsLim> PointsLimit { get; }

        public IMinMax<TSelectionLim> SelectionsLimit { get; }
    }

    internal class LimitsFactory
    {
        public static Limits<bool, bool, bool> CreateIsAddedToParent(BattleScribeXml.Category xml)
        {
            var count4ParentSelections = new MinMax<bool>(
                () => xml.CountTowardsParentMinSelections,
                newMin => xml.CountTowardsParentMinSelections = newMin,
                () => xml.CountTowardsParentMaxSelections,
                newMax => xml.CountTowardsParentMaxSelections = newMax
                );
            var count4ParentPoints = new MinMax<bool>(
                () => xml.CountTowardsParentMinPoints,
                newMin => xml.CountTowardsParentMinPoints = newMin,
                () => xml.CountTowardsParentMaxPoints,
                newMax => xml.CountTowardsParentMaxPoints = newMax
                );
            var count4ParentPercentage = new MinMax<bool>(
                () => xml.CountTowardsParentMinPercentage,
                newMin => xml.CountTowardsParentMinPercentage = newMin,
                () => xml.CountTowardsParentMaxPercentage,
                newMax => xml.CountTowardsParentMaxPercentage = newMax
                );
            return new Limits<bool, bool, bool>(count4ParentSelections,
                count4ParentPoints, count4ParentPercentage);
        }

        public static Limits<bool, bool, bool> CreateIsAddedToParent(BattleScribeXml.ForceType xml)
        {
            var count4ParentSelections = new MinMax<bool>(
                () => xml.CountTowardsParentMinSelections,
                newMin => xml.CountTowardsParentMinSelections = newMin,
                () => xml.CountTowardsParentMaxSelections,
                newMax => xml.CountTowardsParentMaxSelections = newMax
                );
            var count4ParentPoints = new MinMax<bool>(
                () => xml.CountTowardsParentMinPoints,
                newMin => xml.CountTowardsParentMinPoints = newMin,
                () => xml.CountTowardsParentMaxPoints,
                newMax => xml.CountTowardsParentMaxPoints = newMax
                );
            var count4ParentPercentage = new MinMax<bool>(
                () => xml.CountTowardsParentMinPercentage,
                newMin => xml.CountTowardsParentMinPercentage = newMin,
                () => xml.CountTowardsParentMaxPercentage,
                newMax => xml.CountTowardsParentMaxPercentage = newMax
                );
            return new Limits<bool, bool, bool>(count4ParentSelections,
                count4ParentPoints, count4ParentPercentage);
        }

        public static Limits<int, decimal, int> CreateLimits(BattleScribeXml.Category xml)
        {
            var selectionsLimit = new NonNegativeMinMax(
                () => xml.MinSelections,
                newMin => xml.MinSelections = newMin,
                () => xml.MaxSelections,
                newMax => xml.MaxSelections = newMax
                );
            var pointsLimit = new MinMax<decimal>(
                () => xml.MinPoints,
                newMin => xml.MinPoints = newMin,
                () => xml.MaxPoints,
                newMax => xml.MaxPoints = newMax
                );
            var percentageLimit = new NonNegativeMinMax(
                () => xml.MinPercentage,
                newMin => xml.MinPercentage = newMin,
                () => xml.MaxPercentage,
                newMax => xml.MaxPercentage = newMax
                );
            return new Limits<int, decimal, int>(selectionsLimit, pointsLimit, percentageLimit);
        }

        public static Limits<int, decimal, int> CreateLimits(BattleScribeXml.ForceType xml)
        {
            var selectionsLimit = new NonNegativeMinMax(
                () => xml.MinSelections,
                newMin => xml.MinSelections = newMin,
                () => xml.MaxSelections,
                newMax => xml.MaxSelections = newMax
                );
            var pointsLimit = new MinMax<decimal>(
                () => xml.MinPoints,
                newMin => xml.MinPoints = newMin,
                () => xml.MaxPoints,
                newMax => xml.MaxPoints = newMax
                );
            var percentageLimit = new NonNegativeMinMax(
                () => xml.MinPercentage,
                newMin => xml.MinPercentage = newMin,
                () => xml.MaxPercentage,
                newMax => xml.MaxPercentage = newMax
                );
            return new Limits<int, decimal, int>(selectionsLimit, pointsLimit, percentageLimit);
        }
    }
}
