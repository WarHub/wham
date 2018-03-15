using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.JsonUtilities;
using WarHub.ArmouryModel.Workspaces.BattleScribe;

namespace WarHub.ArmouryModel.CliTool.Commands.Convert
{
    public class ConvertXml : CommandBase
    {
        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory]
        public string Source { get; set; }

        [ArgDescription("File to convert."), ArgExistingFile]
        public string File { get; set; }

        [ArgDescription("Directory into which to save conversion results"), ArgExistingDirectory]
        public string Destination { get; set; }

        public void Main()
        {
            SetupLogger();
            var sourceDir = Source == null ? new DirectoryInfo(".") : new DirectoryInfo(Source);
            Log.Debug("Source resolved to {Source}", sourceDir);
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
            var src = destDir.CreateSubdirectory("src");
            Log.Debug("Destination directory resolved to {Destination}", src);
            var foldersByDocumentKind = new Dictionary<XmlDocumentKind, string>
            {
                [XmlDocumentKind.Gamesystem] = "gamesystems",
                [XmlDocumentKind.Catalogue] = "catalogues"
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
                var kindFolder = src.CreateSubdirectory(folderName);
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
        }
    }
}
