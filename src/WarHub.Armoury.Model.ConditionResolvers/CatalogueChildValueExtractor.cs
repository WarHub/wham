// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.ConditionResolvers
{
    using System;
    using Builders;

    public static class CatalogueChildValueExtractor
    {
        public static ConditionChildValue Extract(ICondition condition, IBuilderCore builder)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            return condition.GetParentBuilder(builder).RetrieveChildValue(condition);
        }

        private static IBuilderCore GetParentBuilder(this IConditionCore condition, IBuilderCore builder)
        {
            switch (condition.ParentKind)
            {
                case ConditionParentKind.Roster:
                    return builder.AncestorContext.RosterBuilder;
                case ConditionParentKind.ForceType:
                    return builder.AncestorContext.ForceBuilder;
                case ConditionParentKind.Category:
                    return builder.AncestorContext.CategoryBuilder;
                case ConditionParentKind.DirectParent:
                    return builder.GetDirectParentBuilder();
                case ConditionParentKind.Reference:
                    return builder.FindParentBuilderById(condition.ChildLink.TargetId.Value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition.ParentKind, null);
            }
        }

        private static IBuilderCore GetDirectParentBuilder(this IBuilderCore builder)
        {
            var ctx = builder.AncestorContext;
            if (builder is IEntryBuilder)
            {
                return ctx.EntryBuilder != null ? (IBuilderCore) ctx.EntryBuilder : ctx.CategoryBuilder;
            }
            return ctx.EntryBuilder.AncestorContext.EntryBuilder != null
                ? (IBuilderCore) ctx.EntryBuilder.AncestorContext.EntryBuilder
                : ctx.CategoryBuilder;
        }

        private static IBuilderCore FindParentBuilderById(this IBuilderCore builder, Guid id)
        {
            while (builder.ParentBuilder != null && !builder.IsForEntityId(id))
            {
                builder = builder.ParentBuilder;
            }
            return builder;
        }

        private static ConditionChildValue RetrieveChildValue(this IBuilderCore builder, ICondition condition)
        {
            if (condition.ConditionKind == ConditionKind.InstanceOf)
            {
                return new ConditionChildValue
                {
                    IsInstanceOf = builder.IsForEntityId(condition.ChildLink.TargetId.Value)
                };
            }
            switch (condition.ChildValueUnit)
            {
                case ConditionValueUnit.TotalSelections:
                    return builder.AncestorContext.RosterBuilder.StatAggregate.ChildSelectionsCount;
                case ConditionValueUnit.PointsLimit:
                    return builder.AncestorContext.RosterBuilder.Roster.PointsLimit;
                case ConditionValueUnit.Selections:
                    return builder.StatAggregate.GetSelectionCount(condition.ChildLink.TargetId.Value);
                case ConditionValueUnit.Points:
                    return builder.StatAggregate.GetPointsTotal(condition.ChildLink.TargetId.Value);
                case ConditionValueUnit.Percent:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition.ChildValueUnit,
                        "Invalid for catalogue condition.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition), condition.ChildValueUnit, null);
            }
        }
    }
}
