// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeTests
{
    using System.Linq;
    using Xunit;

    public class SelectionTests : CodedSampleTestBase
    {
        public SelectionTests()
        {
            Initialize();
        }

        public void Initialize()
        {
            DataToolsInitialization();
            SystemInitialization();
            CatalogueInitialization();
            RosterInitialization();
        }

        [Fact]
        public void SubSelectionsDefaultConfigTest()
        {
            var captainEntry = Catalogue.Entries.Single(x => x.Name.Equals("Captain"));
            var captain = Roster.Forces
                .First()
                .CategoryMocks
                .First(x => x.Name.Equals(captainEntry.CategoryLink.Target.Name))
                .Selections
                .First();
            Assert.NotEqual(captain.Selections.Count, 0);
            Assert.NotNull(captain.Selections.SingleOrDefault(x => x.Name.Equals("Armour")));
            Assert.NotNull(captain.Selections.SingleOrDefault(x => x.Name.Equals("Sword")));
        }
    }
}
