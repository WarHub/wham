using Newtonsoft.Json;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    public record GitreeSourceFolder
    (
        [property: JsonProperty("kind")]
        GitreeSourceFolderKind Kind,

        [property: JsonProperty("path")]
        string Subpath
    );
}
