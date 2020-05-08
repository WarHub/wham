using Amadevus.RecordGenerator;
using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectModel
{
    [Record]
    public partial class SourceFolder
    {
        [JsonProperty("kind")]
        public SourceFolderKind Kind { get; }

        [JsonProperty("path")]
        public string Subpath { get; }
    }
}
