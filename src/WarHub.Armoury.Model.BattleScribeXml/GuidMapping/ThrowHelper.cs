// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml.GuidMapping
{
    internal static class ThrowHelper
    {
        //public static void ThrowForAlreadyProcessed(
        //    GameSystem toProcess,
        //    GameSystem alreadyProcessed)
        //{
        //    ThrowForAlreadyProcessed(
        //        InfoFormatter.GameSystemInfo(toProcess),
        //        InfoFormatter.GameSystemInfo(alreadyProcessed));
        //}

        //public static void ThrowForAlreadyProcessed(
        //    Catalogue toProcess,
        //    Catalogue alreadyProcessed)
        //{
        //    ThrowForAlreadyProcessed(
        //        InfoFormatter.CatalogueInfo(toProcess),
        //        InfoFormatter.CatalogueInfo(alreadyProcessed));
        //}

        //public static void ThrowForAlreadyProcessed(
        //    Roster toProcess,
        //    Roster alreadyProcessed)
        //{
        //    ThrowForAlreadyProcessed(
        //        InfoFormatter.RosterInfo(toProcess),
        //        InfoFormatter.RosterInfo(alreadyProcessed));
        //}

        //public static void ThrowForBadGameSystem(string exceptionSourceInfo,
        //    GameSystem gameSystem, string neededGstInfo)
        //{
        //    var msg = string.Format("{0} can't be processed using {1}. {2} is needed",
        //        exceptionSourceInfo,
        //        InfoFormatter.GameSystemInfo(gameSystem),
        //        neededGstInfo);
        //    throw new ProcessingFailedException(msg);
        //}

        //public static void ThrowForCatalogueNotLoaded(Roster roster, Force link)
        //{
        //    var msg = string.Format("{0} (for not-Readonly mode) requires {1} to be loaded first.",
        //        InfoFormatter.RosterInfo(roster),
        //        InfoFormatter.CatalogueInfo(link));
        //    throw new ProcessingFailedException(msg);
        //}

        public static void ThrowForNoGameSystem(string exceptionSourceInfo, string gstInfo)
        {
            var msg = string.Format("{0} can't be processed without {1}.",
                exceptionSourceInfo,
                gstInfo);
            throw new ProcessingFailedException(msg);
        }

        //public static void ThrowForNotProcessed(Roster roster)
        //{
        //    ThrowForNotProcessed(InfoFormatter.RosterInfo(roster));
        //}

        //public static void ThrowForNotProcessed(Catalogue catalogue)
        //{
        //    ThrowForNotProcessed(InfoFormatter.CatalogueInfo(catalogue));
        //}

        //public static void ThrowForNotProcessed(GameSystem gameSystem)
        //{
        //    ThrowForNotProcessed(InfoFormatter.GameSystemInfo(gameSystem));
        //}

        private static void ThrowForAlreadyProcessed(string toProcessInfo, string alreadyProcessedInfo)
        {
            var msg = string.Format("{0} cannot be processed. {1} was already processed in this instance!",
                toProcessInfo,
                alreadyProcessedInfo);
            throw new ProcessingFailedException(msg);
        }

        private static void ThrowForNotProcessed(string sourceInfo)
        {
            var msg = string.Format("Cannot Reprocess {0} because it wasn't Processed in this instance.",
                sourceInfo);
            throw new ProcessingFailedException(msg);
        }
    }
}
