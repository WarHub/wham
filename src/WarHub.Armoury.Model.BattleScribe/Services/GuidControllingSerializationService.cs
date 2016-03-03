namespace WarHub.Armoury.Model.BattleScribe.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using BattleScribeXml.GuidMapping;
    using Repo;
    using XmlGameSystem = BattleScribeXml.GameSystem;
    using XmlCatalogue = BattleScribeXml.Catalogue;
    using XmlRoster = BattleScribeXml.Roster;
    using BattleScribeXmlSerializer = BattleScribeXml.XmlSerializer;

    public sealed class GuidControllingSerializationService : ISerializationService
    {
        internal GuidController GuidController { get; } = new GuidController(GuidControllerMode.Edit);

        private IGameSystemContext SystemContext { get; set; }

        public ICatalogue LoadCatalogue(Stream catalogueXmlStream)
        {
            if (catalogueXmlStream == null)
                throw new ArgumentNullException(nameof(catalogueXmlStream));
            var xmlCatalogue = BattleScribeXmlSerializer.Deserialize<XmlCatalogue>(catalogueXmlStream);
            GuidController.Process(xmlCatalogue);
            var catalogue = new Catalogue(xmlCatalogue);
            return catalogue;
        }

        public IGameSystem LoadGameSystem(Stream gameSystemXmlStream)
        {
            if (gameSystemXmlStream == null)
                throw new ArgumentNullException(nameof(gameSystemXmlStream));
            var xmlGameSystem = BattleScribeXmlSerializer.Deserialize<XmlGameSystem>(gameSystemXmlStream);
            GuidController.Process(xmlGameSystem);
            var gameSystem = new GameSystem(xmlGameSystem);
            SystemContext = gameSystem.Context;
            return gameSystem;
        }

        public IRoster LoadRoster(Stream rosterXmlStream, LoadCatalogueCallback loadCatalogue)
        {
            if (rosterXmlStream == null)
                throw new ArgumentNullException(nameof(rosterXmlStream));
            if (loadCatalogue == null)
                throw new ArgumentNullException(nameof(loadCatalogue));
            var xmlRoster = DeserializeRoster(rosterXmlStream);
            foreach (var catId in ListRequiredCatalogueIds(xmlRoster))
            {
                loadCatalogue(catId);
            }
            GuidController.Process(xmlRoster);
            var roster = new Roster(xmlRoster) {SystemContext = SystemContext};
            return roster;
        }

        public async Task<IRoster> LoadRosterAsync(Stream rosterXmlStream,
            LoadCatalogueAsyncCallback loadCatalogue)
        {
            if (rosterXmlStream == null)
                throw new ArgumentNullException(nameof(rosterXmlStream));
            if (loadCatalogue == null)
                throw new ArgumentNullException(nameof(loadCatalogue));
            var xmlRoster = DeserializeRoster(rosterXmlStream);
            foreach (var catId in ListRequiredCatalogueIds(xmlRoster))
            {
                await loadCatalogue(catId);
            }
            GuidController.Process(xmlRoster);
            var roster = new Roster(xmlRoster) {SystemContext = SystemContext};
            return roster;
        }

        public IRoster LoadRosterReadonly(Stream rosterXmlStream)
        {
            if (rosterXmlStream == null)
                throw new ArgumentNullException(nameof(rosterXmlStream));
            var xmlRoster = DeserializeRoster(rosterXmlStream);
            GuidController.Process(xmlRoster);
            var roster = new Roster(xmlRoster);
            return roster;
        }

        public void SaveRoster(Stream outputStream, IRoster iRoster)
        {
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));
            if (iRoster == null)
                throw new ArgumentNullException(nameof(iRoster));
            if (!(iRoster is Roster))
            {
                throw new NotSupportedException("Roster object is not BattleScribe.Roster"
                                                + " and cannot be serialized by this service.");
            }
            var roster = (Roster) iRoster;
            BattleScribeXmlSerializer.SerializeFormatted(roster.XmlBackend, outputStream);
        }

        internal static List<string> ListRequiredCatalogueIds(XmlRoster xmlRoster)
        {
            return RequirementExtractor.ListRequiredCatalogues(xmlRoster);
        }

        internal XmlRoster DeserializeRoster(Stream rosterXmlStream)
        {
            return BattleScribeXmlSerializer.Deserialize<XmlRoster>(rosterXmlStream);
        }
    }
}
