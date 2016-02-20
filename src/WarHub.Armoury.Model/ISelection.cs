// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    using System.Collections.Generic;

    public interface ISelection : IIdentifiable, INameable,
        IBookIndexable, ISelectionNodeContainer, IForceItem, INotifyPointCostChanged
    {
        uint NumberTaken { get; set; }

        ILinkPath<IEntry> OriginEntryPath { get; }

        /// <summary>
        ///     Optional - exists if entry was chosen from EntryGroup. Then this is that group. The
        ///     property is never null though.
        /// </summary>
        ILinkPath<IGroup> OriginGroupPath { get; }

        /// <summary>
        ///     Point cost of this selection, excluding any children selections.
        /// </summary>
        decimal PointCost { get; }

        IEnumerable<IProfileMock> ProfileMocks { get; }

        IEnumerable<IRuleMock> RuleMocks { get; }

        EntryType Type { get; }
    }
}
