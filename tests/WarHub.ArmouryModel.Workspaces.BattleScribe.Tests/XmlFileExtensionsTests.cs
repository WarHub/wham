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
            const string RepoFilename = "Files/repo.bsr";
            var repo = File.OpenRead(RepoFilename).ReadRepoDistribution();
            Assert.Equal(DataIndexFilename, repo.Index.Filepath);
            Assert.Equal(
                "A_Song_Of_Ice_and_Fire_Miniatures_Game.gst",
                repo.Datafiles.OfType<IDatafileInfo<GamesystemNode>>().Single().Filepath);
            Assert.Equal(3, repo.Datafiles.OfType<IDatafileInfo<CatalogueNode>>().Count());
        }

        [Fact]
        public void WriteRepoDistribution()
        {
            const string DataIndexName = "Test dataindex";
            var gstId = Guid.NewGuid().ToString();
            var catId1 = Guid.NewGuid().ToString();
            var catId2 = Guid.NewGuid().ToString();
            var gstNode = NodeFactory.Gamesystem(id: gstId);
            var original =
                new RepoDistribution(
                    DatafileInfo.Create(
                        DataIndexFilename,
                        NodeFactory.DataIndex(DataIndexName)),
                    (new IDatafileInfo<CatalogueBaseNode>[]
                    {
                        DatafileInfo.Create("gamesystem.gst", gstNode),
                        DatafileInfo.Create("cat1.cat", NodeFactory.Catalogue(gstNode, id: catId1)),
                        DatafileInfo.Create("cat2.cat", NodeFactory.Catalogue(gstNode, id: catId2)),
                    })
                    .ToImmutableArray());
            using (var memory = new MemoryStream())
            {
                original.WriteTo(memory);
                memory.Position = 0;
                var read = memory.ReadRepoDistribution();
                Assert.Equal(DataIndexName, read.Index.GetData().Name);
                Assert.True(
                    read.Datafiles
                    .Select(x => x.GetData().Id)
                    .ToHashSet()
                    .SetEquals(new[] { gstId, catId1, catId2 }));
            }
        }
    }
}
