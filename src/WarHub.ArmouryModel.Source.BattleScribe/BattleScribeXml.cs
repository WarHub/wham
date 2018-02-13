using System.IO;
using System.Xml;

namespace WarHub.ArmouryModel.Source.BattleScribe
{
    /// <summary>
    /// BattleScribe-output-conformance helper class. Provides methods and properties that
    /// enable fully compliant serialization.
    /// </summary>
    public static class BattleScribeXml
    {
        /// <summary>
        /// Loads BattleScribe gamesystem from (unzipped) .gst file from <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Filepath to (unzipped) gamesystem file.</param>
        /// <returns>Gamesystem data model.</returns>
        public static GamesystemNode LoadGamesystem(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return stream.DeserializeGamesystem();
            }
        }

        /// <summary>
        /// Loads BattleScribe gamesystem from (unzipped) .gst file <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Stream of gamesystem file content.</param>
        /// <returns>Gamesystem data model.</returns>
        public static GamesystemNode LoadGamesystem(Stream stream)
        {
            return stream.DeserializeGamesystem();
        }

        /// <summary>
        /// Loads BattleScribe catalogue from (unzipped) .cat file from <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Filepath to (unzipped) catalogue file.</param>
        /// <returns>Catalogue data model.</returns>
        public static CatalogueNode LoadCatalogue(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return stream.DeserializeCatalogue();
            }
        }

        /// <summary>
        /// Loads BattleScribe catalogue from (unzipped) .cat file <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Stream of catalogue file content.</param>
        /// <returns>Catalogue data model.</returns>
        public static CatalogueNode LoadCatalogue(Stream stream)
        {
            return stream.DeserializeCatalogue();
        }


        /// <summary>
        /// Loads BattleScribe roster from (unzipped) .ros file from <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Filepath to (unzipped) roster file.</param>
        /// <returns>Roster data model.</returns>
        public static RosterNode LoadRoster(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return stream.DeserializeRoster();
            }
        }

        /// <summary>
        /// Loads BattleScribe roster from (unzipped) .ros file <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Stream of roster file content.</param>
        /// <returns>Roster data model.</returns>
        public static RosterNode LoadRoster(Stream stream)
        {
            return stream.DeserializeRoster();
        }


        /// <summary>
        /// Loads BattleScribe data index from (unzipped) .xml file from <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Filepath to (unzipped) data index file.</param>
        /// <returns>Data index data model.</returns>
        public static DataIndexNode LoadDataIndex(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return stream.DeserializeDataIndex();
            }
        }

        /// <summary>
        /// Loads BattleScribe data index from (unzipped) .xml file <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">Stream of data index file content.</param>
        /// <returns>Data index data model.</returns>
        public static DataIndexNode LoadDataIndex(Stream stream)
        {
            return stream.DeserializeDataIndex();
        }
    }
}
