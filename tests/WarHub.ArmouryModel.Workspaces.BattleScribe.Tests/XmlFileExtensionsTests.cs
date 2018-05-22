using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.Workspaces.BattleScribe.Tests
{
    public class XmlFileExtensionsTests
    {
        private const string DataIndexFilename = "index.xml";

        [Fact]
        public void ReadExistingRepoDistribution()
        {
            const string repoFilename = "Files/repo.bsr";
            var repo = File.OpenRead(repoFilename).ReadRepoDistribution();
            Assert.Equal(DataIndexFilename, repo.Index.Filepath);
            Assert.Equal(
                "A_Song_Of_Ice_and_Fire_Miniatures_Game.gst",
                repo.Datafiles.OfType<IDatafileInfo<GamesystemNode>>().Single().Filepath);
            Assert.Equal(3, repo.Datafiles.OfType<IDatafileInfo<CatalogueNode>>().Count());
        }

        [Fact]
        public void WriteRepoDistribution()
        {
            const string dataIndexName = "Test dataindex";
            var gstId = Guid.NewGuid().ToString();
            var catId1 = Guid.NewGuid().ToString();
            var catId2 = Guid.NewGuid().ToString();
            var original =
                new RepoDistribution(
                    DatafileInfo.Create(
                        DataIndexFilename,
                        new DataIndexCore.Builder { Name = dataIndexName }.ToImmutable().ToNode()),
                    new IDatafileInfo<CatalogueBaseNode>[]
                    {
                        DatafileInfo.Create(
                            "gamesystem.gst",
                            new GamesystemCore.Builder { Id = gstId }.ToImmutable().ToNode()),
                        DatafileInfo.Create(
                            "cat1.cat",
                            new CatalogueCore.Builder { Id = catId1 }.ToImmutable().ToNode()),
                        DatafileInfo.Create(
                            "cat2.cat",
                            new CatalogueCore.Builder { Id = catId2 }.ToImmutable().ToNode()),
                    }
                    .ToImmutableArray());
            using (var memory = new MemoryStream())
            {
                memory.WriteRepoDistribution(original);
                memory.Position = 0;
                var read = memory.ReadRepoDistribution();
                Assert.Equal(dataIndexName, read.Index.Data.Name);
                Assert.True(
                    read.Datafiles
                    .Select(x => x.Data.Id)
                    .ToHashSet()
                    .SetEquals(new[] { gstId, catId1, catId2 }));
            }
        }
    }
}
