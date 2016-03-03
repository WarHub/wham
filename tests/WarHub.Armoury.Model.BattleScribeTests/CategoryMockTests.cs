// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribeTests
{
    using System;
    using System.Linq;
    using BattleScribe.Services;
    using Repo;
    using Xunit;

    public class CategoryMockTests
    {
        [Fact]
        public void NoCategoryCategoryMockLinkingTest()
        {
            var systemInfo = new GameSystemInfo("system name", Guid.NewGuid().ToString(), 0, "v0", "no book",
                "no author");
            var system = RepoObjectFactory.CreateGameSystem(systemInfo);
            var forceType = system.ForceTypes.AddNew();
            var catalogue = RepoObjectFactory.CreateCatalogue(
                new CatalogueInfo("cat name", Guid.NewGuid().ToString(), 0, systemInfo.RawId, "v0", "no book",
                    "no author"),
                system.Context);
            catalogue.SystemContext = system.Context;
            var entry = catalogue.Entries.AddNew();
            var roster = RepoObjectFactory.CreateRoster(
                new RosterInfo("roster name", Guid.NewGuid().ToString(), systemInfo.RawId, "v0", 0m, 0m),
                system.Context);
            roster.SystemContext = system.Context;
            var force = roster.Forces.AddNew(new ForceNodeArgument(catalogue, forceType));
            Assert.Equal(1, force.CategoryMocks.Count());
            var categoryMock = force.CategoryMocks.Single();
            Assert.Equal(ReservedIdentifiers.NoCategoryName, categoryMock.Name);
            Assert.NotNull(categoryMock.CategoryLink.Target);
        }
    }
}
