// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml.GuidMapping
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Extracts requirements for rosters.
    /// </summary>
    public class RequirementExtractor
    {
        /// <summary>
        ///     Searches for all catalogues mentioned in roster's forces and creates a list of
        ///     (distinct) IDs of these catalogues.
        /// </summary>
        /// <param name="roster">Roster to perform the search in.</param>
        /// <returns>Created list of distinct catalogue IDs.</returns>
        public static List<string> ListRequiredCatalogues(Roster roster)
        {
            var collection = CollectRequiredCatalogues(roster.Forces);
            return collection.Distinct().ToList();
        }

        private static IEnumerable<string> CollectRequiredCatalogues(IEnumerable<Force> forceList)
        {
            var catalogueIdList = new List<string>();
            foreach (var force in forceList)
            {
                catalogueIdList.Add(force.CatalogueId);
                catalogueIdList.AddRange(CollectRequiredCatalogues(force.Forces));
            }
            return catalogueIdList;
        }
    }
}
