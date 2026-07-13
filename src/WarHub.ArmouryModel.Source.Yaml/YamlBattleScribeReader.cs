using System.IO;
using WarHub.ArmouryModel.Source.BattleScribe;
using YamlDotNet.RepresentationModel;

namespace WarHub.ArmouryModel.Source.Yaml;

public static class YamlBattleScribeReader
{
    public static SourceNode? ReadSourceNode(TextReader reader)
    {
        var stream = new YamlStream();
        stream.Load(reader);
        var root = (YamlMappingNode)stream.Documents[0].RootNode;
        var xml = YamlToXmlConverter.Convert(root);
        using var ms = new MemoryStream();
        xml.Save(ms);
        ms.Position = 0;
        return ms.DeserializeSourceNodeAuto();
    }
}
