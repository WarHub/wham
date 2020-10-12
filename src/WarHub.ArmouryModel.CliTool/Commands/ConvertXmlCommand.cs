using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.Gitree;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class ConvertXmlCommand : CommandBase
    {
        public async Task RunAsync(DirectoryInfo source, DirectoryInfo output, string verbosity)
        {
            SetupLogger(verbosity);
            Log.Warning("This command is a Work In Progress. It may not work correctly.");
            Log.Debug("Source resolved to {Source}", source);
            output.Create();
            Log.Debug("Destination directory resolved to {Destination}", output);
            var configInfo = CreateDestinationProjectConfig(source, output);
            var workspace = CreateXmlWorkspace(source);
            configInfo.WriteFile();
            Log.Information("Project configuration saved as {ConfigFile}", configInfo.Filepath);

            await ConvertFilesAsync(configInfo, workspace);
        }

        private async Task ConvertFilesAsync(GitreeWorkspaceOptions gitreeOptions, XmlWorkspace workspace)
        {
            var treeWriter = new GitreeWriter();
            foreach (var document in workspace.GetDocuments(SourceKind.Gamesystem, SourceKind.Catalogue))
            {
                var sourceKind = document.Kind.GetSourceKindOrUnknown();
                var filenameNoExt = Path.GetFileNameWithoutExtension(document.Filepath);
                var folderPath = Path.Combine(gitreeOptions.GetFullPath(sourceKind), filenameNoExt);
                var folder = Directory.CreateDirectory(folderPath);
                Log.Information("Converting file {Name} into {Folder}", filenameNoExt, folder);
                Log.Verbose("- Reading...");
                var sourceNode = await document.GetRootAsync();
                Log.Verbose("- Reading finished. Converting...");
                var gitree = sourceNode.ConvertToGitree();
                Log.Verbose("- Converting finished. Saving to Gitree directory structure...");
                treeWriter.WriteItem(gitree, folder);
                Log.Debug("- Saved");
            }
        }

        private static GitreeWorkspaceOptions CreateDestinationProjectConfig(DirectoryInfo sourceDir, DirectoryInfo destDir)
        {
            var options = GitreeWorkspaceOptions.Create(sourceDir.FullName);
            var destFilepath = Path.Combine(destDir.FullName, Path.GetFileName(options.Filepath));
            return options with
            {
                Filepath = destFilepath,
                SourceDirectories = ImmutableArray.Create(
                    new GitreeSourceFolder(GitreeSourceFolderKind.Catalogues, "src/catalogues"),
                    new GitreeSourceFolder(GitreeSourceFolderKind.Gamesystems, "src/gamesystems"))
            };
        }

        private XmlWorkspace CreateXmlWorkspace(DirectoryInfo sourceDir)
        {
            var workspace = XmlWorkspace.CreateFromDirectory(sourceDir.FullName);
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
    }
}
