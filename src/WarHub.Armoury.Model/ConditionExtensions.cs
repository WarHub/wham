// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ConditionExtensions
    {
        private static IReadOnlyDictionary<ConditionKind, Func<decimal, decimal, bool>> Comparison { get; } = new Dictionary
            <ConditionKind, Func<decimal, decimal, bool>>
        {
            [ConditionKind.AtLeast] = (left, right) => left >= right,
            [ConditionKind.AtMost] = (left, right) => left <= right,
            [ConditionKind.EqualTo] = (left, right) => left == right,
            [ConditionKind.NotEqualTo] = (left, right) => left != right,
            [ConditionKind.GreaterThan] = (left, right) => left > right,
            [ConditionKind.LessThan] = (left, right) => left < right
        };

        public static bool Is(this decimal value, ConditionKind conditionKind, decimal other)
            => conditionKind.SatisfiedBy(value, other);

        public static bool SatisfiedBy(this ConditionKind conditionKind, decimal left, decimal right)
        {
            if (!Comparison.ContainsKey(conditionKind))
                throw new ArgumentOutOfRangeException(nameof(conditionKind), conditionKind,
                    "Invalid value! Only arithmetic kinds allowed.");
            return Comparison[conditionKind](left, right);
        }

        public static bool SatisfiedBy(this ICondition condition, ConditionChildValue childValue)
        {
            return condition.ConditionKind == ConditionKind.InstanceOf
                ? childValue.IsInstanceOf
                : Comparison[condition.ConditionKind](childValue.Number, condition.ChildValue);
        }

        public static ConditionChildKind GetChildKindFromGuid(this Guid childGuid)
        {
            string special;
            return ReservedIdentifiers.NameDictionary.TryGetValue(childGuid, out special)
                ? special.GetChildKindFromName()
                : ConditionChildKind.Reference;
        }

        public static ConditionChildKind GetChildKindFromName(this string childName)
        {
            ConditionChildKind conditionChildKind;
            return childName.TryParseXml(out conditionChildKind) ? conditionChildKind : ConditionChildKind.Reference;
        }

        public static ConditionParentKind GetParentKindFromGuid(this Guid parentGuid)
        {
            string special;
            return ReservedIdentifiers.NameDictionary.TryGetValue(parentGuid, out special)
                ? special.GetParentKindFromName()
                : ConditionParentKind.Reference;
        }

        public static ConditionParentKind GetParentKindFromName(this string parentName)
        {
            ConditionParentKind conditionParentKind;
            return parentName.TryParseXml(out conditionParentKind) ? conditionParentKind : ConditionParentKind.Reference;
        }

        public static INameable GetConditionParentRef(this ICatalogueCondition condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            return condition.ParentKind != ConditionParentKind.Reference
                ? null
                : condition.GetLinkTarget(condition.ParentLink);
        }

        public static INameable GetConditionChildRef(this ICatalogueCondition condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            return condition.ChildKind != ConditionChildKind.Reference
                ? null
                : condition.GetLinkTarget(condition.ChildLink);
        }

        private static INameable GetLinkTarget(this ICatalogueContextProvider condition, ILink queriedLink)
        {
            return condition.Context.Entries.FirstOrDefault(queriedLink.LinksTo) ??
                   condition.Context.EntryLinks.FirstOrDefault(queriedLink.LinksTo)?.Target ??
                   condition.Context.Groups.FirstOrDefault(queriedLink.LinksTo) ??
                   (INameable) condition.Context.GroupLinks.FirstOrDefault(queriedLink.LinksTo)?.Target;
        }
    }
}
