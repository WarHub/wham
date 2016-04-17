namespace WarHub.Armoury.Model.ConditionResolvers
{
    using System;
    using System.Linq;
    using Builders;

    public class CategoryChildValueExtractor
    {
        /// <summary>
        ///     Extracts child value based on <see cref="ICategoryModifier" />'s condition.
        /// </summary>
        /// <param name="conditionCore">Describes what child value to extract.</param>
        /// <param name="builder">Provides context to extract value from.</param>
        /// <returns>Extracted value.</returns>
        public static ConditionChildValue Extract(IConditionCore conditionCore, IBuilderCore builder)
        {
            if (conditionCore == null) throw new ArgumentNullException(nameof(conditionCore));
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            switch (conditionCore.ParentKind)
            {
                case ConditionParentKind.Roster:
                    return GetChildValue4Roster(conditionCore, builder);
                case ConditionParentKind.Reference:
                    return GetChildValue4CategoryRef(conditionCore, builder);
                case ConditionParentKind.ForceType:
                case ConditionParentKind.Category:
                case ConditionParentKind.DirectParent:
                    throw new ArgumentOutOfRangeException(nameof(conditionCore.ParentKind), conditionCore.ParentKind,
                        "Invalid for game system condition.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionCore.ParentKind), conditionCore.ParentKind,
                        null);
            }
        }

        private static ConditionChildValue GetChildValue4CategoryRef(IConditionCore conditionCore, IBuilderCore builder)
        {
            var categoryBuilder =
                builder.AncestorContext.ForceBuilder.CategoryBuilders.First(
                    bldr => bldr.CategoryMock.CategoryLink.TargetIdValuesAreEqual(conditionCore.ParentLink));
            switch (conditionCore.ChildValueUnit)
            {
                case ConditionValueUnit.Points:
                    return categoryBuilder.StatAggregate.PointsTotal;
                case ConditionValueUnit.Percent:
                    return 100*categoryBuilder.StatAggregate.PointsTotal/
                           builder.AncestorContext.RosterBuilder.Roster.PointsLimit;
                case ConditionValueUnit.Selections:
                    return categoryBuilder.StatAggregate.ChildSelectionsCount;
                case ConditionValueUnit.TotalSelections:
                case ConditionValueUnit.PointsLimit:
                    throw new ArgumentOutOfRangeException(nameof(conditionCore.ChildValueUnit),
                        conditionCore.ChildValueUnit, "Invalid for game system condition with Category-ref parent.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionCore.ChildValueUnit),
                        conditionCore.ChildValueUnit, null);
            }
        }

        private static ConditionChildValue GetChildValue4Roster(IConditionCore conditionCore, IBuilderCore builder)
        {
            switch (conditionCore.ChildValueUnit)
            {
                case ConditionValueUnit.PointsLimit:
                    return builder.AncestorContext.RosterBuilder.Roster.PointsLimit;
                case ConditionValueUnit.TotalSelections:
                    return builder.AncestorContext.RosterBuilder.StatAggregate.ChildSelectionsCount;
                case ConditionValueUnit.Selections:
                case ConditionValueUnit.Points:
                case ConditionValueUnit.Percent:
                    throw new ArgumentOutOfRangeException(nameof(conditionCore.ChildValueUnit),
                        conditionCore.ChildValueUnit,
                        "Invalid for game system condition with Roster parent.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionCore.ChildValueUnit),
                        conditionCore.ChildValueUnit, null);
            }
        }
    }
}
