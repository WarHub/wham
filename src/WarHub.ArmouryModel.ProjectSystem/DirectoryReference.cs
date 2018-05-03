using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectSystem
{
    [Record]
    public partial class DirectoryReference
    {
        [JsonProperty("kind")]
        public DirectoryReferenceKind Kind { get; }

        [JsonProperty("path")]
        public string Path { get; }

        public override string ToString()
        {
            return $"{Kind}@{Path}";
        }
    }
}