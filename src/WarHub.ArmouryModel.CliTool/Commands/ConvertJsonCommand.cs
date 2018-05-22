using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.JsonInfrastructure;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Source.BattleScribe;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.JsonFolder;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class ConvertJsonCommand : CommandBase
    {
        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory, ArgRequired]
        public string Source { get; set; }

        [ArgDescription("Directory into which to save conversion results"), ArgRequired]
        public string Destination { get; set; }

        public void Main()
        {
            SetupLogger();
            var workspace = JsonWorkspace.CreateFromPath(Source);
            Log.Debug("Source resolved to {RootPath}", workspace.Root.Path);
            var destDir = new DirectoryInfo(Destination);
            Log.Debug("Destination resolved to {Destination}", destDir);
            destDir.Create();
            var jsonBlobTreeVisitor = new JsonBlobTreeVisitor();
            var converter = new JsonBlobTreeToSourceNodeConverter();
            var serializer = new BattleScribeXmlSerializer();
            Log.Information("Converting...");
            foreach (var sourceDirRef in workspace.ProjectConfiguration.SourceDirectories)
            {
                Log.Debug("Converting JSON trees in SourceDir {DirectoryReference}", sourceDirRef);
                var subfolderPath = Path.Combine(workspace.Root.Path, sourceDirRef.Path);
                foreach (var sourceTreeFolder in GetSubfolders())
                {
                    Log.Debug("Converting JSON tree '{SubfolderName}' from {DirRef}", sourceTreeFolder.Name, sourceDirRef);
                    Log.Verbose("- Loading JSON tree...");
                    var blobItem = jsonBlobTreeVisitor.VisitItemFolder(sourceTreeFolder);
                    Log.Verbose("- Loading finished. Converting to monolitic model...");
                    var node = converter.ParseItem(blobItem);
                    Log.Verbose("- Conversion finished. Saving XML file...");
                    var (serialize, extension) = GetXmlKindUtilities(node);
                    var filename = Path.Combine(destDir.FullName, sourceTreeFolder.Name + extension);
                    using (var fileStream = File.Create(filename))
                    {
                        serialize(fileStream);
                    }
                    Log.Verbose("- Saved.");
                }
                IEnumerable<JsonFolder> GetSubfolders()
                {
                    return Directory
                       .EnumerateDirectories(subfolderPath)
                       .Select(x => new JsonFolder(new DirectoryInfo(x), workspace));
                }
            }
            Log.Information("Finished converting.");

            WaitForReadKey();

            (Action<Stream> serialize, string extension) GetXmlKindUtilities(SourceNode node)
            {
                return node.MatchOnType<(Action<Stream>, string)>(
                    gamesystemMap: gst => (gst.Serialize, XmlFileExtensions.Gamesystem),
                    catalogueMap: cat => (cat.Serialize, XmlFileExtensions.Catalogue));
            }
        }
    }
}
