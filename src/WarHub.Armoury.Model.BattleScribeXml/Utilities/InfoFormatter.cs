// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeXml
{
    internal static class InfoFormatter
    {
        public static string CatalogueInfo(Catalogue catalogue)
        {
            return CatalogueInfo(catalogue.Name, catalogue.Id);
        }

        public static string CatalogueInfo(string catalogueId)
        {
            return $"Catalogue (id={{{catalogueId}}})";
        }

        public static string CatalogueInfo(Force link)
        {
            return CatalogueInfo(link.CatalogueName, link.CatalogueId);
        }

        public static string CatalogueInfo(string catalogueName, string catalogueId)
        {
            return $"Catalogue \'{catalogueName}\' id={{{catalogueId}}}";
        }

        public static string GameSystemInfo(GameSystem gameSystem)
        {
            return GameSystemInfo(gameSystem.Name, gameSystem.Id);
        }

        public static string GameSystemInfo(string gameSystemName, string gameSystemId)
        {
            return $"Game System \'{gameSystemName}\' id={{{gameSystemId}}}";
        }

        public static string GameSystemInfo(string gameSystemId)
        {
            return $"Game System (id={{{gameSystemId}}})";
        }

        public static string RosterInfo(Roster roster)
        {
            return $"Roster \'{roster.Name}\' id={{{roster.Id}}}";
        }
    }
}
