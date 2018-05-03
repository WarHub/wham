using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.JsonInfrastructure;
using WarHub.ArmouryModel.ProjectSystem;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.JsonFolder;

namespace WarHub.ArmouryModel.CliTool.Commands.Convert
{
    public class ConvertXml : CommandBase
    {
        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory]
        public string Source { get; set; }

        [ArgDescription("File to convert."), ArgExistingFile]
        public string File { get; set; }

        [ArgDescription("Directory into which to save conversion results.")]
        public string Destination { get; set; }

        public void Main()
        {
            SetupLogger();
            var sourceDir = Source == null ? new DirectoryInfo(".") : new DirectoryInfo(Source);
            Log.Debug("Source resolved to {Source}", sourceDir);
            var projectConfig = new ConvertedJsonProjectConfigurationProvider().Create(sourceDir.FullName);
            var workspace = File == null
                ? XmlWorkspace.CreateFromDirectory(sourceDir.FullName)
                : new XmlWorkspace(sourceDir.GetFiles(File));
            Log.Debug("Found {Count} documents at source", workspace.Documents.Length);
            foreach (var (kind, docs) in workspace.DocumentsByKind)
            {
                Log.Debug("- {Count}x {Kind}", docs.Length, kind);
            }
            foreach (var doc in workspace.Documents)
            {
                Log.Verbose("- {Kind} {Name} at {Path}", doc.Kind, doc.Name, doc.Path);
            }
            var destDir = new DirectoryInfo(Destination ?? ".");
            destDir.Create();
            Log.Debug("Destination directory resolved to {Destination}", destDir);

            var serializer = JsonUtilities.CreateSerializer();
            var projectConfigFilename = $"{sourceDir.Name}{ProjectConfiguration.FileExtension}";
            var projectConfigFilepath = Path.Combine(destDir.FullName, projectConfigFilename);
            using (var projectConfigurationWriter = System.IO.File.CreateText(projectConfigFilepath))
            {
                serializer.Serialize(projectConfigurationWriter, projectConfig);
            }
            Log.Debug("Project file created as {ProjectFile}", projectConfigFilename);

            var foldersByDocumentKind = new Dictionary<XmlDocumentKind, string>
                {
                    [XmlDocumentKind.Gamesystem] = projectConfig.GetRefForKind(DirectoryReferenceKind.Gamesystems).Path,
                    [XmlDocumentKind.Catalogue] = projectConfig.GetRefForKind(DirectoryReferenceKind.Catalogues).Path
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
                    var filenameNoExt = Path.GetFileNameWithoutExtension(document.Path);
                    var documentFolder = kindFolder.CreateSubdirectory(filenameNoExt);
                    Log.Information("Converting file {Name} into {Folder}", filenameNoExt, documentFolder);
                    Log.Verbose("- Reading...");
                    var root = document.GetRoot();
                    Log.Verbose("- Reading finished. Converting...");
                    var tree = treeConverter.Visit(root);
                    Log.Verbose("- Converting finished. Saving to JSON directory structure...");
                    xmlToJsonWriter.WriteItem(tree, documentFolder);
                    Log.Verbose("- Saved");
                }
            }
            WaitForReadKey();
        }
        
        private class ConvertedJsonProjectConfigurationProvider : JsonFolderProjectConfigurationProvider
        {
            protected override ImmutableArray<DirectoryReference> DefaultDirectoryReferences { get; } =
                ImmutableArray.Create(
                    new DirectoryReference(DirectoryReferenceKind.Catalogues, "src/catalogues"),
                    new DirectoryReference(DirectoryReferenceKind.Gamesystems, "src/gamesystems"));

            protected override ProjectConfiguration CreateDefault(string path)
            {
                return new ProjectConfiguration(ToolsetVersion, DefaultDirectoryReferences);
            }
        }
    }
}
