// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeTests
{
    using BattleScribe.Services;

    public class CodedSampleTestBase
    {
        protected ICatalogue Catalogue { get; private set; }

        protected IRoster Roster { get; private set; }

        protected SampleObjectFactory SampleFactory { get; private set; }

        protected IGameSystem System { get; private set; }

        protected void CatalogueInitialization()
        {
            Catalogue = SampleFactory.SampleCatalogue;
        }

        protected void DataToolsInitialization()
        {
            SampleFactory = new SampleObjectFactory();
        }

        protected void RosterInitialization()
        {
            Roster = SampleFactory.SampleRoster;
        }

        protected void SystemInitialization()
        {
            System = SampleFactory.SampleGameSystem;
        }
    }
}
