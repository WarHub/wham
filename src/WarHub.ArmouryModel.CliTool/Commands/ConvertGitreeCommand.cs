using System.IO;
using WarHub.ArmouryModel.Source.BattleScribe;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.Gitree;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class ConvertGitreeCommand : CommandBase
    {
        public void Run(DirectoryInfo source, DirectoryInfo output, string verbosity)
        {
            SetupLogger(verbosity);
            var workspace = GitreeWorkspace.CreateFromPath(source.FullName);
            Log.Debug("Source resolved to {RootPath}", workspace.RootPath);
            Log.Debug("Destination resolved to {Destination}", output);
            output.Create();
            Log.Information("Converting...");
            foreach (var datafile in workspace.Datafiles)
            {
                var fileDir = new FileInfo(datafile.Filepath).Directory;
                Log.Debug("Converting Gitree '{SubfolderName}' from {DirRef}", fileDir.Name, fileDir.Parent.FullName);
                Log.Verbose("- Loading Gitree...");
                var node = datafile.GetData();
                Log.Verbose("- Loading finished. Saving XML file...");
                var extension = node.GetXmlDocumentKindOrUnknown().GetXmlFileExtension();
                var filename = Path.Combine(output.FullName, fileDir.Name + extension);
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
