using System.Collections.Immutable;
using System.IO;
using Newtonsoft.Json;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Workspaces.Gitree.Serialization;

namespace WarHub.ArmouryModel.Workspaces.Gitree
{
    public record GitreeWorkspaceOptions
    {
        public const string DefaultSourcePath = "src";

        public const string DefaultOutputPath = "artifacts";

        public const string FileExtension = ".whamproj";

        private static ImmutableArray<GitreeSourceFolder> DefaultDirectoryReferences { get; } =
            ImmutableArray.Create(new GitreeSourceFolder(GitreeSourceFolderKind.All, DefaultSourcePath));

        [JsonIgnore]
        public string Filepath { get; init; } = "project" + FileExtension;

        [JsonProperty("toolset")]
        public string ToolsetVersion { get; init; } = ProjectToolset.Version;

        [JsonProperty("src")]
        public ImmutableArray<GitreeSourceFolder> SourceDirectories { get; init; } = ImmutableArray<GitreeSourceFolder>.Empty;

        [JsonProperty("out")]
        public string OutputPath { get; init; } = DefaultOutputPath;

        public static GitreeWorkspaceOptions Create(string path)
        {
            // TODO version check
            var raw = CreateRaw();
            return SanitizeConfiguration(raw);

            GitreeWorkspaceOptions CreateRaw()
            {
                return File.Exists(path)
                    ? ReadFromFile(path)
                    : CreateDefault(path);
            }
            static GitreeWorkspaceOptions CreateDefault(string directory)
            {
                return new GitreeWorkspaceOptions
                {
                    Filepath = Path.Combine(directory, CreateDefaultFilename())
                };
                string CreateDefaultFilename()
                {
                    var dir = new DirectoryInfo(directory);
                    var folderName = dir.Parent != null ? dir.Name : "project";
                    var filename = folderName + FileExtension;
                    return filename;
                }
            }
            static GitreeWorkspaceOptions SanitizeConfiguration(GitreeWorkspaceOptions raw)
            {
                return raw with
                {
                    ToolsetVersion = raw.ToolsetVersion ?? ProjectToolset.Version,
                    SourceDirectories = raw.SourceDirectories.IsDefaultOrEmpty ? DefaultDirectoryReferences : raw.SourceDirectories,
                    OutputPath = string.IsNullOrWhiteSpace(raw.OutputPath) ? DefaultOutputPath : raw.OutputPath
                };
            }
            static GitreeWorkspaceOptions ReadFromFile(string filepath)
            {
                return ReadText() with { Filepath = filepath };

                GitreeWorkspaceOptions ReadText()
                {
                    using var streamReader = File.OpenText(filepath);
                    var serializer = JsonUtilities.CreateSerializer();
                    using var jsonReader = new JsonTextReader(streamReader);
                    return serializer.Deserialize<GitreeWorkspaceOptions>(jsonReader);
                }
            }
        }
    }
}
