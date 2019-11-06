using System.Collections.Immutable;
using System.IO;
using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectModel
{
    public abstract class ProjectConfigurationProviderBase : IProjectConfigurationProvider
    {
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

        protected static string CreateDefaultFilename(string directory)
        {
            var dir = new DirectoryInfo(directory);
            var folderName = dir.Parent != null ? dir.Name : "project";
            var filename = folderName + ProjectConfiguration.FileExtension;
            return filename;
        }

        protected virtual ProjectConfiguration CreateDefaultCore(string directory)
        {
            return new ProjectConfiguration(
                ProjectToolset.Version,
                DefaultDirectoryReferences,
                ProjectConfiguration.DefaultOutputPath,
                ProviderType);
        }

        public abstract ProjectFormatProviderType ProviderType { get; }

        protected abstract ImmutableArray<SourceFolder> DefaultDirectoryReferences { get; }

        protected virtual ProjectConfiguration SanitizeConfiguration(ProjectConfiguration raw)
        {
            return raw.Update(
                raw.ToolsetVersion ?? ProjectToolset.Version,
                raw.SourceDirectories.IsDefaultOrEmpty ? DefaultDirectoryReferences : raw.SourceDirectories,
                string.IsNullOrWhiteSpace(raw.OutputPath) ? ProjectConfiguration.DefaultOutputPath : raw.OutputPath,
                raw.FormatProvider);
        }
    }
}
