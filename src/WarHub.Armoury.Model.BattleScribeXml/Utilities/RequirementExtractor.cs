// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Extracts requirements for rosters.
    /// </summary>
    public static class RequirementExtractor
    {
        /// <summary>
        ///     Searches for all catalogues mentioned in roster's forces and creates a list of
        ///     (distinct) IDs of these catalogues.
        /// </summary>
        /// <param name="roster">Roster to perform the search in.</param>
        /// <returns>Created list of distinct catalogue IDs.</returns>
        public static List<string> GetRequiredCatalogues(this Roster roster)
        {
            if (roster == null)
            {
                throw new ArgumentNullException(nameof(roster));
            }
            return roster.Forces.SelectWithNestedForces(force => force.CatalogueId).Distinct().ToList();
        }
    }
}
