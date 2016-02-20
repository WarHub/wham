// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    /// <summary>
    ///     Condition is met if and only if: <see cref="IConditionCore.ParentKind" /> (if
    ///     <see cref="ConditionParentKind.Reference" />, <see cref="IConditionCore.ParentLink" />) has/is
    ///     <see cref="ConditionKind" />
    ///     <see cref="IConditionCore.ChildValue" /> <see cref="IConditionCore.ChildValueUnit" /> of
    ///     <see cref="IConditionCore.ChildKind" /> (if <see cref="ConditionChildKind.Reference" />,
    ///     <see cref="IConditionCore.ChildLink" />).
    /// </summary>
    public interface ICondition : IConditionCore, ICloneable<ICondition>
    {
        ConditionKind ConditionKind { get; set; }
    }
}
