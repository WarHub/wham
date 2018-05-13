using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectSystem
{
    public enum ProjectFormatProviderType
    {
        [JsonProperty("json")]
        JsonFolders = 0,
        [JsonProperty("xml-cat")]
        XmlCatalogues = 1,
    }
}