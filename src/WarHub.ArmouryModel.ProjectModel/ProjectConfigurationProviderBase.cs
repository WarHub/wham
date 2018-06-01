using System.Collections.Immutable;
using System.IO;
using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectModel
{
    public class ProjectToolset
    {
        public static string Version { get; } = "v0.0-alpha";

        public static string BattleScribeFormatVersion { get; } = "2.01";
    }

    public abstract class ProjectConfigurationProviderBase : IProjectConfigurationProvider
    {
        public string CurrentToolsetVersion => ProjectToolset.Version;

        public ProjectConfigurationInfo Create(string path)
        {
            return CreateCore(path);
        }

        protected virtual ProjectConfigurationInfo CreateCore(string path)
        {
            // TODO version check
            var raw = CreateRaw();
            var sanitized = SanitizeConfiguration(raw.Configuration);
            return raw.WithConfiguration(sanitized);

            ProjectConfigurationInfo CreateRaw()
            {
                return File.Exists(path)
                    ? ReadFromFile(path)
                    : CreateDefault(path);
            }
        }

        private static ProjectConfiguration ReadText(TextReader reader)
        {
            var serializer = JsonUtilities.CreateSerializer();
            using (var jsonReader = new JsonTextReader(reader))
            {
                return serializer.Deserialize<ProjectConfiguration>(jsonReader);
            }
        }

        public static ProjectConfigurationInfo ReadFromFile(string filepath)
        {
            using (var streamReader = File.OpenText(filepath))
            {
                var config = ReadText(streamReader);
                return new ProjectConfigurationInfo(filepath, config);
            }
        }

        protected virtual ProjectConfigurationInfo CreateDefault(string directory)
        {
            var config = CreateDefaultCore(directory);
            var filepath = Path.Combine(directory, CreateDefaultFilename(directory));
            return new ProjectConfigurationInfo(filepath, config);
        }

        protected string CreateDefaultFilename(string directory)
        {
            var dir = new DirectoryInfo(directory);
            var folderName = dir.Parent != null ? dir.Name : "project";
            var filename = folderName + ProjectConfiguration.FileExtension;
            return filename;
        }

        protected abstract ProjectConfiguration CreateDefaultCore(string directory);

        protected abstract ImmutableArray<SourceFolder> DefaultDirectoryReferences { get; }

        protected virtual ProjectConfiguration SanitizeConfiguration(ProjectConfiguration raw)
        {
            return raw.Update(
                raw.ToolsetVersion ?? CurrentToolsetVersion,
                raw.SourceDirectories.IsDefaultOrEmpty ? DefaultDirectoryReferences : raw.SourceDirectories,
                string.IsNullOrWhiteSpace(raw.OutputPath) ? ProjectConfiguration.DefaultOutputPath : raw.OutputPath,
                raw.FormatProvider);
        }
    }
}