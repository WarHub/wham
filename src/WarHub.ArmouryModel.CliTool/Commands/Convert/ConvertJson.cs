using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Optional;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.JsonUtilities;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Source.BattleScribe;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.JsonFolder;

namespace WarHub.ArmouryModel.CliTool.Commands.Convert
{
    public class ConvertJson : CommandBase
    {
        [ArgDescription("Specify to run continuously, watching source directory/file for changes.")]
        public bool Watch { get; set; }

        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory, ArgRequired]
        public string Source { get; set; }

        [ArgDescription("Directory into which to save conversion results"), ArgRequired]
        public string Destination { get; set; }

        public void Main()
        {
            SetupLogger();
            Log.Debug("Source resolved to {Source}", Source);
            var workspace = JsonWorkspace.CreateFromDirectory(Source);
            var destDir = new DirectoryInfo(Destination);
            Log.Debug("Destination resolved to {Destination}", destDir);
            destDir.Create();
            var reviver = new JsonBlobTreeReviver();
            var converter = new BlobTreeToSourceRootConverter();
            var serializer = new BattleScribeXmlSerializer();
            Log.Information("Converting...");
            foreach (var sourceKindFolder in workspace.Root.GetFolders())
            {
                Log.Debug("Converting JSON trees in {KindFolder}", sourceKindFolder.Path);
                foreach (var sourceTreeFolder in sourceKindFolder.GetFolders())
                {
                    Log.Debug("Converting JSON tree @ {TreeFolder}", sourceTreeFolder);
                    Log.Verbose("- Loading JSON tree...");
                    var blobItem = reviver.VisitItemFolder(sourceTreeFolder);
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
            }
            Log.Information("Finished converting.");

            (Action<Stream> serialize, string extension) GetXmlKindUtilities(SourceNode node)
            {
                return node.MatchOnType<(Action<Stream>, string)>(
                    gamesystemMap: gst => (gst.Serialize, XmlFileExtensions.Gamesystem),
                    catalogueMap: cat => (cat.Serialize, XmlFileExtensions.Catalogue));
            }
        }
    }
}
