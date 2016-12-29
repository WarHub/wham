// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml.GuidMapping
{
    internal static class InfoFormatter
    {
        //public static string CatalogueInfo(Catalogue catalogue)
        //{
        //    return CatalogueInfo(catalogue.Name, catalogue.Id);
        //}

        public static string CatalogueInfo(string catalogueId)
        {
            return string.Format("Catalogue (id={0}{1}{2})",
                '{', catalogueId, '}');
        }

        //public static string CatalogueInfo(Force link)
        //{
        //    return CatalogueInfo(link.CatalogueName, link.CatalogueId);
        //}

        public static string CatalogueInfo(string catalogueName, string catalogueId)
        {
            return string.Format("Catalogue '{0}' id={1}{2}{3}",
                catalogueName, '{', catalogueId, '}');
        }

        //public static string GameSystemInfo(GameSystem gameSystem)
        //{
        //    return GameSystemInfo(gameSystem.Name, gameSystem.Id);
        //}

        public static string GameSystemInfo(string gameSystemName, string gameSystemId)
        {
            return string.Format("Game System '{0}' id={1}{2}{3}",
                gameSystemName, '{', gameSystemId, '}');
        }

        public static string GameSystemInfo(string gameSystemId)
        {
            return string.Format("Game System (id={0}{1}{2})",
                '{', gameSystemId, '}');
        }

        //public static string RosterInfo(Roster roster)
        //{
        //    return string.Format("Roster '{0}' id={1}{2}{3}",
        //        roster.Name, '{', roster.Id, '}');
        //}
    }
}
