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
    public class ConvertXml
    {
        [ArgDescription("Specify to run continuously, watching source directory/file for changes.")]
        public bool Watch { get; set; }

        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory]
        public string Source { get; set; }

        [ArgDescription("File to convert."), ArgExistingFile]
        public string File { get; set; }

        [ArgDescription("Directory into which to save conversion results"), ArgExistingDirectory]
        public string Destination { get; set; }

        public void Main()
        {
            var sourceDir = Source == null ? new DirectoryInfo(".") : new DirectoryInfo(Source);
            var workspace = File == null
                ? XmlWorkspace.CreateFromDirectory(sourceDir.FullName)
                : new XmlWorkspace(sourceDir.GetFiles(File));
            var destDir = new DirectoryInfo(Destination ?? ".");
            var src = destDir.CreateSubdirectory("src");
            var foldersByDocumentKind = new Dictionary<XmlDocumentKind, string>
            {
                [XmlDocumentKind.Gamesystem] = "gamesystems",
                [XmlDocumentKind.Catalogue] = "catalogues"
            }.ToImmutableDictionary();
            var treeConverter = new DatablobTreeConverter();
            var xmlToJsonWriter = new XmlToJsonWriter();
            foreach (var (kind, folderName) in foldersByDocumentKind)  
            {
                workspace.DocumentsByKind.TryGetValue(kind, out var documents);
                if (documents.IsDefaultOrEmpty)
                {
                    continue;
                }
                var kindFolder = src.CreateSubdirectory(folderName);
                foreach (var document in documents)
                {
                    var filenameNoExt = Path.GetFileNameWithoutExtension(document.Path);
                    var documentFolder = kindFolder.CreateSubdirectory(filenameNoExt);
                    var root = document.GetRoot();
                    var tree = treeConverter.Visit(root);
                    xmlToJsonWriter.WriteNode(tree, documentFolder);
                }
            }
        }
    }
}
