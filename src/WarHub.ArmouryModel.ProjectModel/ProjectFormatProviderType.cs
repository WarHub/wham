using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectModel
{
    public enum ProjectFormatProviderType
    {
        [JsonProperty("json")]
        JsonFolders = 0,
        [JsonProperty("xml-cat")]
        XmlCatalogues = 1,
    }
}