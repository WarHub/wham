namespace WarHub.Armoury.Model.BattleScribe.Services
{
    using System;
    using BattleScribeXml.GuidMapping;
    using Repo;
    using XmlCatalogue = BattleScribeXml.Catalogue;
    using XmlRoster = BattleScribeXml.Roster;
    using XmlGameSystem = BattleScribeXml.GameSystem;

    /// <summary>
    ///     Creates empty Model.Common objects with BattleScribe implementation.
    /// </summary>
    public static class RepoObjectFactory
    {
        private const string GuidFormat = GuidController.GuidFormat;

        /// <summary>
        ///     Creates new Catalogue object against provided information, ready to use. The Catalogue
        ///     implementation is backed by directly-xml-serializable classes.
        /// </summary>
        /// <param name="catInfo">The basic info of catalogue to be created.</param>
        /// <param name="systemContext">
        ///     The context of Game System in which this catalogue will be created. Also this context is
        ///     already set in Catalogue.
        /// </param>
        /// <returns>Empty created catalogue.</returns>
        public static ICatalogue CreateCatalogue(CatalogueInfo catInfo, IGameSystemContext systemContext)
        {
            var system = systemContext.GameSystem;
            var catXml = new XmlCatalogue
            {
                AuthorName = catInfo.AuthorName,
                BattleScribeVersion = catInfo.OriginProgramVersion,
                GameSystemGuid = system.Id.Value,
                GameSystemId = system.Id.Value.ToString(GuidFormat),
                GameSystemRevision = system.Revision,
                Guid = GuidParse(catInfo.RawId),
                Id = catInfo.RawId,
                Name = catInfo.Name
            };
            var catalogue = new Catalogue(catXml);
            catalogue.SystemContext = systemContext;
            return catalogue;
        }

        /// <summary>
        ///     Creates new Game System object against provided information, ready to use. The System
        ///     implementation is backed by directly-xml-serializable classes.
        /// </summary>
        /// <param name="gameSystemInfo">The basic info of game system to be created.</param>
        /// <returns>Empty created game system.</returns>
        public static IGameSystem CreateGameSystem(GameSystemInfo gameSystemInfo)
        {
            var gstXml = new XmlGameSystem
            {
                AuthorName = gameSystemInfo.AuthorName,
                BattleScribeVersion = gameSystemInfo.OriginProgramVersion,
                Guid = GuidParse(gameSystemInfo.RawId),
                Id = gameSystemInfo.RawId,
                Name = gameSystemInfo.Name
            };
            return new GameSystem(gstXml);
        }

        /// <summary>
        ///     Creates new Roster object against provided information, ready to use. The Roster
        ///     implementation is backed by directly-xml-serializable classes.
        /// </summary>
        /// <param name="rosterInfo">The basic info of roster to be created.</param>
        /// <param name="systemContext">
        ///     The system context in which this roster will function. This context is already set on
        ///     returned roster.
        /// </param>
        /// <returns>Empty created roster.</returns>
        public static IRoster CreateRoster(RosterInfo rosterInfo, IGameSystemContext systemContext)
        {
            var system = systemContext.GameSystem;
            var rosterXml = new XmlRoster
            {
                BattleScribeVersion = rosterInfo.OriginProgramVersion,
                GameSystemGuid = system.Id.Value,
                GameSystemId = system.Id.Value.ToString(GuidFormat),
                GameSystemName = system.Name,
                GameSystemRevision = system.Revision,
                Guid = GuidParse(rosterInfo.RawId),
                Id = rosterInfo.RawId,
                Name = rosterInfo.Name,
                PointsLimit = rosterInfo.PointsLimit
            };
            var roster = new Roster(rosterXml);
            roster.SystemContext = systemContext;
            return roster;
        }

        private static Guid GuidParse(string id)
        {
            return Guid.ParseExact(id, GuidFormat);
        }
    }
}
