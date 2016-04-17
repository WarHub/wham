namespace WarHub.Armoury.Model.ModifierAppliers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GeneralLimits = ILimits<int, decimal, int>;

    public static class CategoryApplierExtensions
    {
        public static void Apply(this GeneralLimits @this, ICategoryModifier modifier,
            IConditionResolver resolver)
        {
            if (modifier.Repetition.IsActive)
            {
                var count = resolver.CountRepeats(modifier.Repetition);
                for (var i = 0; i < count; i++)
                {
                    @this.ApplyCore(modifier);
                }
            }
            else if ((!modifier.Conditions.Any() ||
                      modifier.Conditions.All(resolver.IsMet)) &&
                     (!modifier.ConditionGroups.Any() ||
                      modifier.ConditionGroups.All(resolver.IsMet)))
            {
                @this.ApplyCore(modifier);
            }
        }

        public static void Apply(this GeneralLimits @this, IEnumerable<ICategoryModifier> modifiers,
            IConditionResolver resolver)
        {
            foreach (var modifier in modifiers)
            {
                @this.Apply(modifier, resolver);
            }
        }

        private static void ApplyCore(this GeneralLimits @this, ICategoryModifier modifier)
        {
            switch (modifier.Field)
            {
                case LimitField.MinSelections:
                    @this.SelectionsLimit.Min = @this.SelectionsLimit.Min.Modified(modifier.Action,
                        Convert.ToInt32(modifier.Value));
                    break;
                case LimitField.MaxSelections:
                    @this.SelectionsLimit.Max = @this.SelectionsLimit.Max.Modified(modifier.Action,
                        Convert.ToInt32(modifier.Value));
                    break;
                case LimitField.MinPoints:
                    @this.PointsLimit.Min = @this.PointsLimit.Min.Modified(modifier.Action, modifier.Value);
                    break;
                case LimitField.MaxPoints:
                    @this.PointsLimit.Max = @this.PointsLimit.Max.Modified(modifier.Action, modifier.Value);
                    break;
                case LimitField.MinPercentage:
                    @this.PercentageLimit.Min = @this.PercentageLimit.Min.Modified(modifier.Action,
                        Convert.ToInt32(modifier.Value));
                    break;
                case LimitField.MaxPercentage:
                    @this.PercentageLimit.Max = @this.PercentageLimit.Max.Modified(modifier.Action,
                        Convert.ToInt32(modifier.Value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static decimal Modified(this decimal @this, CategoryModifierAction action, decimal value)
        {
            switch (action)
            {
                case CategoryModifierAction.Increment:
                    return @this + value;
                case CategoryModifierAction.Decrement:
                    return @this - value;
                case CategoryModifierAction.Set:
                    return value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private static int Modified(this int @this, CategoryModifierAction action, int value)
        {
            switch (action)
            {
                case CategoryModifierAction.Increment:
                    return @this + value;
                case CategoryModifierAction.Decrement:
                    return @this - value;
                case CategoryModifierAction.Set:
                    return value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
    }
}
