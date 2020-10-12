using Newtonsoft.Json;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
#pragma warning disable CA1801 // Parameter x of method .ctor is never used. Remove the parameter or use it in the method body.
    public record GitreeSourceFolder
    (
        [property: JsonProperty("kind")]
        GitreeSourceFolderKind Kind,

        [property: JsonProperty("path")]
        string Subpath
    );
#pragma warning restore CA1801
}
