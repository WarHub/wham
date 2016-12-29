// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml.GuidMapping
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Checks for all appropriate requirements on root object level. That includes required
    ///     (mentioned/linked) other data objects and whether object already was processed.
    /// </summary>
    //internal class RequirementValidator
    //{
    //    public RequirementValidator(GuidController controller)
    //    {
    //        Controller = controller;
    //    }

    //    private GuidController Controller { get; }

    //    //public void ValidateCanReprocess(Roster roster)
    //    //{
    //    //    if (!Controller.Rosters.ContainsKey(roster.Id))
    //    //    {
    //    //        ThrowHelper.ThrowForNotProcessed(roster);
    //    //    }
    //    //}

    //    //public void ValidateRequirements(Roster roster)
    //    //{
    //    //    CheckIfAlreadyProcessed(roster);
    //    //    if (Controller.Mode != GuidControllerMode.RosterReadonly)
    //    //    {
    //    //        CheckGameSystemFor(roster);
    //    //        CheckRequiredCatalogues(roster);
    //    //    }
    //    //}

    //    //public void ValidateRequirements(Catalogue catalogue)
    //    //{
    //    //    CheckIfAlreadyProcessed(catalogue);
    //    //    CheckGameSystemFor(catalogue);
    //    //}

    //    //public void ValidateRequirements(GameSystem gameSystem)
    //    //{
    //    //    CheckIfAlreadyProcessed(gameSystem);
    //    //}

    //    //private void CheckGameSystem(Func<string> sourceInfo, string gstName, string gstId)
    //    //{
    //    //    var gameSystem = Controller.GameSystem;
    //    //    if (gameSystem == null)
    //    //    {
    //    //        ThrowHelper.ThrowForNoGameSystem(
    //    //            sourceInfo(),
    //    //            InfoFormatter.GameSystemInfo(gstName, gstId));
    //    //    }
    //    //    if (gameSystem.Id != gstId)
    //    //    {
    //    //        ThrowHelper.ThrowForBadGameSystem(
    //    //            sourceInfo(),
    //    //            gameSystem,
    //    //            InfoFormatter.GameSystemInfo(gstName, gstId));
    //    //    }
    //    //}

    //    //private void CheckGameSystemFor(Roster roster)
    //    //{
    //    //    CheckGameSystem(() => InfoFormatter.RosterInfo(roster),
    //    //        roster.GameSystemName, roster.GameSystemId);
    //    //}

    //    //private void CheckGameSystemFor(Catalogue catalogue)
    //    //{
    //    //    CheckGameSystem(() => InfoFormatter.CatalogueInfo(catalogue),
    //    //        null, catalogue.GameSystemId);
    //    //}

    //    //private void CheckIfAlreadyProcessed(Roster roster)
    //    //{
    //    //    var rosters = Controller.Rosters;
    //    //    if (rosters.ContainsKey(roster.Id))
    //    //    {
    //    //        ThrowHelper.ThrowForAlreadyProcessed(roster, rosters[roster.Id]);
    //    //    }
    //    //}

    //    //private void CheckIfAlreadyProcessed(Catalogue catalogue)
    //    //{
    //    //    var catalogues = Controller.Catalogues;
    //    //    if (catalogues.ContainsKey(catalogue.Id))
    //    //    {
    //    //        ThrowHelper.ThrowForAlreadyProcessed(catalogue, catalogues[catalogue.Id]);
    //    //    }
    //    //}

    //    //private void CheckIfAlreadyProcessed(GameSystem gameSystem)
    //    //{
    //    //    if (Controller.GameSystem != null)
    //    //    {
    //    //        ThrowHelper.ThrowForAlreadyProcessed(gameSystem, Controller.GameSystem);
    //    //    }
    //    //}

    //    //private void CheckRequiredCatalogues(Roster roster)
    //    //{
    //    //    CheckRequiredCatalogues(roster.Forces, roster);
    //    //}

    //    //private void CheckRequiredCatalogues(List<Force> forceList, Roster roster)
    //    //{
    //    //    foreach (var force in forceList)
    //    //    {
    //    //        if (!Controller.Catalogues.ContainsKey(force.CatalogueId))
    //    //        {
    //    //            ThrowHelper.ThrowForCatalogueNotLoaded(roster, force);
    //    //        }
    //    //        CheckRequiredCatalogues(force.Forces, roster);
    //    //    }
    //    //}
    //}
}
