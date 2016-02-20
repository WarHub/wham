// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.ComponentModel;

    /// <summary>
    ///     Describes base elements of condition.
    /// </summary>
    public interface IConditionCore : INotifyPropertyChanged
    {
        ConditionChildKind ChildKind { get; set; }

        ILink ChildLink { get; }

        decimal ChildValue { get; set; }

        ConditionValueUnit ChildValueUnit { get; set; }

        ConditionParentKind ParentKind { get; set; }

        ILink ParentLink { get; }
    }
}
