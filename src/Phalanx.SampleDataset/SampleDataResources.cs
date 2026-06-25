using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Workspaces.BattleScribe;

namespace Phalanx.SampleDataset;

public sealed class SampleDataResources
{
    public static string[] GetDataResourceNames() =>
        typeof(SampleDataResources).Assembly.GetManifestResourceNames();

    public static Stream? OpenDataResource(string name) =>
        typeof(SampleDataResources).Assembly.GetManifestResourceStream(name);

    public static XmlDocument LoadXmlDocumentFromResource(string name)
    {
        using var stream = OpenDataResource(name);
        var node = stream!.LoadSourceAuto(name);
        var datafileInfo = DatafileInfo.Create(name, node);
        return XmlDocument.Create(datafileInfo);
    }

    public static XmlWorkspace CreateXmlWorkspace() =>
        XmlWorkspace.CreateFromDocuments(
            GetDataResourceNames()
            .Select(x => LoadXmlDocumentFromResource(x))
            .ToImmutableArray());
}
