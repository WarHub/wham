using System.Collections.Immutable;
using System.IO;
using Newtonsoft.Json;

namespace WarHub.ArmouryModel.ProjectModel
{
    public class ProjectToolset
    {
        public static string Version { get; } = "v0.0-alpha";
    }

    public abstract class ProjectConfigurationProviderBase : IProjectConfigurationProvider
    {
        public string CurrentToolsetVersion => ProjectToolset.Version;

        public ProjectConfiguration Create(string path)
        {
            return CreateCore(path);
        }

        protected virtual ProjectConfiguration CreateCore(string path)
        {
            // TODO version check
            var rawConfiguration = CreateRaw();
            var sanitized = SanitizeConfiguration(rawConfiguration);
            return sanitized;

            ProjectConfiguration CreateRaw()
            {
                return !File.Exists(path) ? CreateDefault(path) : ReadFile(path);
            }
        }

        protected ProjectConfiguration ReadText(TextReader reader)
        {
            var serializer = JsonUtilities.CreateSerializer();
            using (var jsonReader = new JsonTextReader(reader))
            {
                return serializer.Deserialize<ProjectConfiguration>(jsonReader);
            }
        }

        protected virtual ProjectConfiguration ReadFile(string path)
        {
            using (var streamReader = File.OpenText(path))
            {
                return ReadText(streamReader);
            }
        }

        protected abstract ProjectConfiguration CreateDefault(string path);

        protected abstract ImmutableArray<SourceFolder> DefaultDirectoryReferences { get; }

        protected virtual ProjectConfiguration SanitizeConfiguration(ProjectConfiguration raw)
        {
            return raw.Update(
                raw.ToolsetVersion ?? CurrentToolsetVersion,
                !raw.SourceDirectories.IsDefaultOrEmpty ? raw.SourceDirectories : DefaultDirectoryReferences,
                string.IsNullOrWhiteSpace(raw.OutputPath) ? ProjectConfiguration.DefaultOutputPath : raw.OutputPath,
                raw.FormatProvider);
        }
    }
}