using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.JsonInfrastructure;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.JsonFolder;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class ConvertXmlCommand : CommandBase
    {
        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory]
        public string Source { get; set; }

        [ArgDescription("File to convert."), ArgExistingFile]
        public string File { get; set; }

        [ArgDescription("Directory into which to save conversion results.")]
        public string Destination { get; set; }

        protected override void MainCore()
        {
            var sourceDir = ResolveSourceDir();
            var projectConfig = CreateProjectConfig(sourceDir);
            var workspace = CreateWorkspace(sourceDir);
            var destDir = ResolveDestinationDir();
            SaveProjectConfig(sourceDir, projectConfig, destDir);

            var foldersByDocumentKind = new Dictionary<XmlDocumentKind, string>
            {
                [XmlDocumentKind.Gamesystem] = projectConfig.GetSourceFolder(SourceKind.Gamesystem).Path,
                [XmlDocumentKind.Catalogue] = projectConfig.GetSourceFolder(SourceKind.Catalogue).Path
            }.ToImmutableDictionary();
            var treeConverter = new SourceNodeToJsonBlobTreeConverter();
            var xmlToJsonWriter = new JsonBlobTreeWriter();
            foreach (var (kind, folderName) in foldersByDocumentKind)
            {
                workspace.DocumentsByKind.TryGetValue(kind, out var documents);
                if (documents.IsDefaultOrEmpty)
                {
                    continue;
                }
                var kindFolder = destDir.CreateSubdirectory(folderName);
                Log.Debug("Converting documents of kind {Kind}, saving into {Folder}", kind, kindFolder);
                foreach (var document in documents)
                {
                    var filenameNoExt = Path.GetFileNameWithoutExtension(document.Filepath);
                    var documentFolder = kindFolder.CreateSubdirectory(filenameNoExt);
                    Log.Information("Converting file {Name} into {Folder}", filenameNoExt, documentFolder);
                    Log.Verbose("- Reading...");
                    var root = document.GetRoot();
                    Log.Verbose("- Reading finished. Converting...");
                    var tree = treeConverter.Visit(root);
                    Log.Verbose("- Converting finished. Saving to JSON directory structure...");
                    xmlToJsonWriter.WriteItem(tree, documentFolder);
                    Log.Debug("- Saved");
                }
            }
        }

        private void SaveProjectConfig(DirectoryInfo sourceDir, ProjectConfiguration projectConfig, DirectoryInfo destDir)
        {
            var serializer = JsonUtilities.CreateSerializer();
            var projectConfigFilename = $"{sourceDir.Name}{ProjectConfiguration.FileExtension}";
            var projectConfigFilepath = Path.Combine(destDir.FullName, projectConfigFilename);
            using (var projectConfigurationWriter = System.IO.File.CreateText(projectConfigFilepath))
            {
                serializer.Serialize(projectConfigurationWriter, projectConfig);
            }
            Log.Information("Project file created as {ProjectFile}", projectConfigFilename);
        }

        private static ProjectConfiguration CreateProjectConfig(DirectoryInfo sourceDir)
        {
            return new ConvertedJsonProjectConfigurationProvider().Create(sourceDir.FullName);
        }

        private XmlWorkspace CreateWorkspace(DirectoryInfo sourceDir)
        {
            var workspace = File == null
                            ? XmlWorkspace.CreateFromDirectory(sourceDir.FullName)
                            : new XmlWorkspace(sourceDir.GetFiles(File).Select(XmlFileExtensions.GetDatafileInfo));
            Log.Debug("Found {Count} documents at source", workspace.Documents.Length);
            foreach (var (kind, docs) in workspace.DocumentsByKind)
            {
                Log.Debug("- {Count}x {Kind}", docs.Length, kind);
            }
            foreach (var doc in workspace.Documents)
            {
                Log.Verbose("- {Kind} {Name} at {Path}", doc.Kind, doc.Name, doc.Filepath);
            }

            return workspace;
        }

        private DirectoryInfo ResolveDestinationDir()
        {
            var destDir = new DirectoryInfo(Destination ?? ".");
            destDir.Create();
            Log.Debug("Destination directory resolved to {Destination}", destDir);
            return destDir;
        }

        private DirectoryInfo ResolveSourceDir()
        {
            var sourceDir = Source == null ? new DirectoryInfo(".") : new DirectoryInfo(Source);
            Log.Debug("Source resolved to {Source}", sourceDir);
            return sourceDir;
        }

        private class ConvertedJsonProjectConfigurationProvider : JsonFolderProjectConfigurationProvider
        {
            protected override ImmutableArray<SourceFolder> DefaultDirectoryReferences { get; } =
                ImmutableArray.Create(
                    new SourceFolder(SourceFolderKind.Catalogues, "src/catalogues"),
                    new SourceFolder(SourceFolderKind.Gamesystems, "src/gamesystems"));

            protected override ProjectConfiguration CreateDefault(string path)
            {
                return base.CreateDefault(path).WithSourceDirectories(DefaultDirectoryReferences);
            }
        }
    }
}
