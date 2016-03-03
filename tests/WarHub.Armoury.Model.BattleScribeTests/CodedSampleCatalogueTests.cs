// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeTests
{
    using Xunit;

    public class CodedSampleCatalogueTests : CodedSampleTestBase
    {
        public CodedSampleCatalogueTests()
        {
            Initialize();
        }

        [Fact]
        public void CatalogueInitializationTest()
        {
            CatalogueInitialization();
        }

        public void Initialize()
        {
            DataToolsInitialization();
            SystemInitialization();
        }
    }
}
