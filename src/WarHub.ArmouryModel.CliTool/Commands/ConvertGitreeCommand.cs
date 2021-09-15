using System;
using System.IO;
using System.Threading.Tasks;
using WarHub.ArmouryModel.Source.BattleScribe;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.Gitree;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class ConvertGitreeCommand : CommandBase
    {
        public async Task RunAsync(DirectoryInfo source, DirectoryInfo output, string verbosity)
        {
            SetupLogger(verbosity);
            var workspace = GitreeWorkspace.CreateFromPath(source.FullName);
            Log.Warning("This command is a Work In Progress. It may not work correctly.");
            Log.Debug("Source resolved to {RootPath}", workspace.RootPath);
            Log.Debug("Destination resolved to {Destination}", output);
            output.Create();
            Log.Information("Converting...");
            foreach (var datafile in workspace.Datafiles)
            {
                var fileDir = new FileInfo(datafile.Filepath).Directory;
                if (fileDir is null)
                    throw new NotSupportedException($"File must have a parent directory ({datafile.Filepath}).");
                Log.Debug("Converting Gitree '{SubfolderName}' from '{DirRef}'", fileDir.Name, fileDir.Parent?.FullName);
                Log.Verbose("- Loading Gitree...");
                var node = await datafile.GetDataAsync()
                    ?? throw new NotSupportedException($"Failed to retrieve data node from {datafile.Filepath}");
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
