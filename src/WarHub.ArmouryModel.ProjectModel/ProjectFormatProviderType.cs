using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectModel
{
    public enum ProjectFormatProviderType
    {
        [JsonProperty("gitree")]
        Gitree = 0,
        [JsonProperty("xml")]
        XmlCatalogues = 1,
    }
}