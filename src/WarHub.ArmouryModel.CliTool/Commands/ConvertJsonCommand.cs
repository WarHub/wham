using System.IO;
using PowerArgs;
using WarHub.ArmouryModel.Source.BattleScribe;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.Gitree;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class ConvertJsonCommand : CommandBase
    {
        [ArgShortcut("s")]
        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory, ArgRequired]
        public string Source { get; set; }

        [ArgShortcut("d")]
        [ArgDescription("Directory into which to save conversion results"), ArgRequired]
        public string Destination { get; set; }

        protected override void MainCore()
        {
            var workspace = JsonWorkspace.CreateFromPath(Source);
            Log.Debug("Source resolved to {RootPath}", workspace.RootPath);
            var destDir = new DirectoryInfo(Destination);
            Log.Debug("Destination resolved to {Destination}", destDir);
            destDir.Create();
            Log.Information("Converting...");
            foreach (var datafile in workspace.Datafiles)
            {
                var fileDir = new FileInfo(datafile.Filepath).Directory;
                Log.Debug("Converting JSON tree '{SubfolderName}' from {DirRef}", fileDir.Name, fileDir.Parent.FullName);
                Log.Verbose("- Loading JSON tree...");
                var node = datafile.GetData();
                Log.Verbose("- Loading finished. Saving XML file...");
                var extension = node.GetXmlDocumentKindOrUnknown().GetXmlFileExtension();
                var filename = Path.Combine(destDir.FullName, fileDir.Name + extension);
                using (var fileStream = File.Create(filename))
                {
                    node.Serialize(fileStream);
                }
                Log.Verbose("- Saved.");
            }
            Log.Information("Finished converting.");
        }
    }
}
