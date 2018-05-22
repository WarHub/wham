using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectModel
{
    public enum SourceFolderKind
    {
        [JsonProperty("all")]
        All,

        [JsonProperty("catalogues")]
        Catalogues,

        [JsonProperty("gamesystems")]
        Gamesystems
    }
}