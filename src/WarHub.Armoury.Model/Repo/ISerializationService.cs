// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.Repo
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    ///     Immediately loads catalogue with provided <paramref name="catalogueId" /> . When this method
    ///     returns, catalogue has to loaded.
    /// </summary>
    /// <param name="catalogueId">Identifies catalogue to be loaded.</param>
    public delegate Task LoadCatalogueAsyncCallback(string catalogueId);

    /// <summary>
    ///     Immediately loads catalogue with provided <paramref name="catalogueId" /> . When this method
    ///     returns, catalogue has to loaded.
    /// </summary>
    /// <param name="catalogueId">Identifies catalogue to be loaded.</param>
    public delegate void LoadCatalogueCallback(string catalogueId);

    /// <summary>
    ///     Provides methods for (de)serialization of repository objects such as <see cref="IRoster" /> ,
    ///     <see cref="ICatalogue" /> or <see cref="IGameSystem" /> . Loads and links referenced and
    ///     required objects, such as catalogues and gamesystems for rosters for non-readonly access. etc.
    /// </summary>
    public interface ISerializationService
    {
        /// <summary>
        ///     Deserializes catalogue from provided stream.
        /// </summary>
        /// <param name="catalogueXmlStream">Contains catalogue xml.</param>
        /// <returns>Catalogue read from stream.</returns>
        /// <exception cref="RequiredDataMissingException">When game system wasn't yet loaded.</exception>
        /// <exception cref="ArgumentNullException">When argument is null.</exception>
        ICatalogue LoadCatalogue(Stream catalogueXmlStream);

        /// <summary>
        ///     Deserializes game system from provided stream.
        /// </summary>
        /// <param name="gameSystemXmlStream">Contains game system xml.</param>
        /// <returns>Game system read from stream.</returns>
        /// <exception cref="ArgumentNullException">When argument is null.</exception>
        IGameSystem LoadGameSystem(Stream gameSystemXmlStream);

        /// <summary>
        ///     Deserializes roster read from provided stream. Loads and links to referenced catalogues
        ///     and game system. Assert game system is loaded before invoking this method.
        /// </summary>
        /// <param name="rosterXmlStream">Contains roster xml.</param>
        /// <param name="loadCatalogue">
        ///     Callback to load catalogue identified by provided string parameter. May be called
        ///     multiple times with different arguments, if referenced catalogue wasn't loaded yet.
        /// </param>
        /// <returns>Roster read from stream and linked with its references.</returns>
        /// <exception cref="RequiredDataMissingException">
        ///     When one of referenced catalogues failed to be read with provided function or if game
        ///     system wasn't loaded yet.
        /// </exception>
        /// <exception cref="ArgumentNullException">When any argument is null.</exception>
        IRoster LoadRoster(Stream rosterXmlStream, LoadCatalogueCallback loadCatalogue);

        /// <summary>
        ///     Deserializes roster read from provided stream. Loads and links to referenced catalogues
        ///     and game system. Assert game system is loaded before invoking this method.
        /// </summary>
        /// <param name="rosterXmlStream">Contains roster xml.</param>
        /// <param name="loadCatalogue">
        ///     Callback to load catalogue identified by provided string parameter. May be called
        ///     multiple times with different arguments, if referenced catalogue wasn't loaded yet.
        /// </param>
        /// <returns>Roster read from stream and linked with its references.</returns>
        /// <exception cref="RequiredDataMissingException">
        ///     When one of referenced catalogues failed to be read with provided function or if game
        ///     system wasn't loaded yet.
        /// </exception>
        /// <exception cref="ArgumentNullException">When any argument is null.</exception>
        Task<IRoster> LoadRosterAsync(Stream rosterXmlStream, LoadCatalogueAsyncCallback loadCatalogue);

        /// <summary>
        ///     Deserializes roster read from provided stream. Its not linked to catalogues/game system.
        /// </summary>
        /// <param name="rosterXmlStream">Contains roster xml.</param>
        /// <returns>Roster read from stream.</returns>
        /// <exception cref="ArgumentNullException">When argument is null.</exception>
        IRoster LoadRosterReadonly(Stream rosterXmlStream);

        /// <summary>
        ///     Serializes roster to xml stream.
        /// </summary>
        /// <param name="outputStream">Stream to which serialization is done.</param>
        /// <param name="roster">Roster to be serialized.</param>
        /// <exception cref="ArgumentNullException">When any argument is null.</exception>
        void SaveRoster(Stream outputStream, IRoster roster);

        /// <summary>
        ///     Serializes catalogue to xml stream.
        /// </summary>
        /// <param name="outputStream">Stream to which serialization is done.</param>
        /// <param name="catalogue">Catalogue to be serialized.</param>
        /// <exception cref="ArgumentNullException">When any argument is null.</exception>
        void SaveCatalogue(Stream outputStream, ICatalogue catalogue);

        /// <summary>
        ///     Serializes game system to xml stream.
        /// </summary>
        /// <param name="outputStream">Stream to which serialization is done.</param>
        /// <param name="gameSystem">Game system to be serialized.</param>
        /// <exception cref="ArgumentNullException">When any argument is null.</exception>
        void SaveGameSystem(Stream outputStream, IGameSystem gameSystem);
    }
}
