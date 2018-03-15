using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectSystem
{
    public enum DirectoryReferenceKind
    {
        [JsonProperty("all")]
        All,

        [JsonProperty("catalogues")]
        Catalogues,

        [JsonProperty("gameSystems")]
        GameSystems
    }
}