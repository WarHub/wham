namespace WarHub.Armoury.Model.ConditionResolvers
{
    using Builders;

    /// <summary>
    ///     Using provided <paramref name="builder" />, child value required to evaluate condition result is extracted,
    ///     according to <paramref name="conditionCore" />.
    /// </summary>
    /// <typeparam name="TBuilder">Type of builder used.</typeparam>
    /// <param name="conditionCore">Provides value extraction details, kind and method.</param>
    /// <param name="builder">Provides data required to extract value.</param>
    /// <returns>Condition's child value (calculated, not provided by <see cref="ICondition.ChildValue" />).</returns>
    public delegate ConditionChildValue ExtractChildValue<in TBuilder>(ICondition conditionCore, TBuilder builder)
        where TBuilder : IBuilderCore;
}
