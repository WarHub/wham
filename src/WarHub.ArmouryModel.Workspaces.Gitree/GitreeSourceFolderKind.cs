using Newtonsoft.Json;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    public enum GitreeSourceFolderKind
    {
        [JsonProperty("all")]
        All,

        [JsonProperty("catalogues")]
        Catalogues,

        [JsonProperty("gamesystems")]
        Gamesystems
    }
}
