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
        // Only the first document in the stream is read; multi-document YAML
        // streams (`---` separated) are not supported. Intentional for now.
        if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode root)
            throw new YamlBattleScribeFormatException("expected a YAML mapping as the document root");
        var xml = YamlToXmlConverter.Convert(root);
        using var ms = new MemoryStream();
        xml.Save(ms);
        ms.Position = 0;
        return ms.DeserializeSourceNodeAuto();
    }
}
