using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.Source.Yaml.Tests;

public class Wh40kDataIntegrationTests
{
    private static string? DataDir => Environment.GetEnvironmentVariable("WH40K11E_DATA");

    public static IEnumerable<object[]> AllDataFiles() =>
        DataDir is { } dir && Directory.Exists(dir)
            ? Directory.EnumerateFiles(dir, "*.yaml").Select(f => new object[] { f })
            : [];

    [Theory]
    [MemberData(nameof(AllDataFiles))]
    public void Every_wh40k11e_file_loads(string path)
    {
        using var reader = File.OpenText(path);
        var node = YamlBattleScribeReader.ReadSourceNode(reader);
        Assert.NotNull(node);
        Assert.True(node is CatalogueNode or GamesystemNode, $"unexpected root {node.GetType().Name} in {path}");

        // Sanity check: a strict-mode load can still produce a structurally hollow node if a
        // real-data scalar key collides with neither a known attribute nor TextElementNames -
        // it's silently dropped by XML deserialization instead of throwing. Guard against that
        // by requiring a non-empty root Name and at least one populated child collection.
        var rootName = node switch
        {
            CatalogueNode cat => cat.Name,
            GamesystemNode sys => sys.Name,
            _ => null,
        };
        Assert.False(string.IsNullOrEmpty(rootName), $"root node has no Name in {path}");

        var hasPopulatedCollection = node.ChildrenInfos().Any(c => c.IsList && c.Node.Children().Any());
        Assert.True(hasPopulatedCollection, $"root node has zero populated child collections in {path}");
    }

    [Fact]
    public void Integration_data_present_or_skipped()
    {
        Assert.SkipWhen(DataDir is null, "WH40K11E_DATA not set - integration data skipped.");
        Assert.True(Directory.Exists(DataDir));
    }
}
