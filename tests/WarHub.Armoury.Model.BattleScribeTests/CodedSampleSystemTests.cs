// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeTests
{
    using Xunit;

    public class CodedSampleSystemTests : CodedSampleTestBase
    {
        public CodedSampleSystemTests()
        {
            Initialize();
        }

        public void Initialize()
        {
            DataToolsInitialization();
        }

        [Fact]
        public void SystemInitializationTest()
        {
            SystemInitialization();
        }
    }
}
