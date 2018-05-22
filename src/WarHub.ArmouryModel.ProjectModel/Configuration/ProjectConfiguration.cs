using System.Collections.Immutable;
using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectModel
{
    [Record]
    public partial class ProjectConfiguration
    {
        public const string FileExtension = ".whamproj";

        public const string DefaultOutputPath = "artifacts";

        [JsonProperty("toolset")]
        public string ToolsetVersion { get; }

        [JsonProperty("src")]
        public ImmutableArray<SourceFolder> SourceDirectories { get; }

        [JsonProperty("out")]
        public string OutputPath { get; }

        [JsonProperty("format")]
        public ProjectFormatProviderType FormatProvider { get; }
    }
}