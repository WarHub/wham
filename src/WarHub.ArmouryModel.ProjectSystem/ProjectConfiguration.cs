using System.Collections.Immutable;
using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectSystem
{
    [Record]
    public partial class ProjectConfiguration
    {
        public const string FileExtension = ".whamproj";

        [JsonProperty("toolset")]
        public string ToolsetVersion { get; }

        [JsonProperty("src")]
        public ImmutableArray<DirectoryReference> SourceDirectories { get; }
    }
}