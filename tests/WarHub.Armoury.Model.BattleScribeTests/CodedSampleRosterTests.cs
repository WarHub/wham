// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeTests
{
    using Xunit;

    public class CodedSampleRosterTests : CodedSampleTestBase
    {
        public CodedSampleRosterTests()
        {
            Initialize();
        }

        public void Initialize()
        {
            DataToolsInitialization();
            SystemInitialization();
            CatalogueInitialization();
        }

        [Fact]
        public void RosterInitializationTest()
        {
            RosterInitialization();
        }
    }
}
