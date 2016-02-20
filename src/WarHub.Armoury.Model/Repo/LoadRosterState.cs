// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    /// <summary>
    ///     Describes state of roster loading.
    /// </summary>
    public enum LoadRosterState
    {
        /// <summary>
        ///     Indicates empty state, no loading is currently performed.
        /// </summary>
        NoState = 0,

        /// <summary>
        ///     Loading prepared to run.
        /// </summary>
        Initiated = 1,

        /// <summary>
        ///     Game System is loaded into memory.
        /// </summary>
        LoadingGameSystem = 10,

        /// <summary>
        ///     Roster is being deserialized.
        /// </summary>
        DeserializingRoster = 20,

        /// <summary>
        ///     List of catalogues required by roster is being created.
        /// </summary>
        IndexingRequiredCatalogues = 30,

        /// <summary>
        ///     Catalogues required by roster are loaded (additional info available).
        /// </summary>
        LoadingRequiredCatalogues = 40,

        /// <summary>
        ///     Roster is being connected with catalogues.
        /// </summary>
        PreparingRoster = 90,

        /// <summary>
        ///     Loading is finished.
        /// </summary>
        Finished = 100
    }
}
