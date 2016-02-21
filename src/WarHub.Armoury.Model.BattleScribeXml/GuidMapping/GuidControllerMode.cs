// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml.GuidMapping
{
    /// <summary>
    ///     Describes what checks are made on each Process call.
    /// </summary>
    public enum GuidControllerMode
    {
        /// <summary>
        ///     On Process(Roster) disables checking for catalogues and game system.
        /// </summary>
        RosterReadonly,

        /// <summary>
        ///     Checks whether object wasn't loaded yet, and if all required parents are loaded.
        /// </summary>
        Edit
    }
}
