// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model
{
    public interface IRoster : IIdentifiable, INameable, IProgramVersioned,
        IForceNodeContainer, IRosterContextProvider
    {
        IIdLink<IGameSystem> GameSystemLink { get; }

        string GameSystemName { get; }

        uint GameSystemRevision { get; }

        /// <summary>
        ///     Total point cost of all selections. Updated automatically.
        /// </summary>
        decimal PointCost { get; }

        decimal PointsLimit { get; set; }

        IGameSystemContext SystemContext { get; set; }
    }
}
