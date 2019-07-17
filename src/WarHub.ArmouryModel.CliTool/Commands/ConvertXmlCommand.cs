using System.Collections.Immutable;
using System.IO;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.Gitree;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class ConvertXmlCommand : CommandBase
    {
        public void Run(DirectoryInfo source, DirectoryInfo output, string verbosity)
        {
            SetupLogger(verbosity);
            Log.Debug("Source resolved to {Source}", source);
            output.Create();
            Log.Debug("Destination directory resolved to {Destination}", output);
            var configInfo = CreateDestinationProjectConfig(source, output);
            var workspace = CreateXmlWorkspace(source);
            configInfo.WriteFile();
            Log.Information("Project configuration saved as {ConfigFile}", configInfo.Filepath);

            ConvertFiles(configInfo, workspace);
        }

        private void ConvertFiles(ProjectConfigurationInfo configInfo, XmlWorkspace workspace)
        {
            var treeWriter = new GitreeWriter();
            foreach (var document in workspace.GetDocuments(SourceKind.Gamesystem, SourceKind.Catalogue))
            {
                var sourceKind = document.Kind.GetSourceKindOrUnknown();
                var filenameNoExt = Path.GetFileNameWithoutExtension(document.Filepath);
                var folderPath = Path.Combine(configInfo.GetFullPath(sourceKind), filenameNoExt);
                var folder = Directory.CreateDirectory(folderPath);
                Log.Information("Converting file {Name} into {Folder}", filenameNoExt, folder);
                Log.Verbose("- Reading...");
                var sourceNode = document.GetRoot();
                Log.Verbose("- Reading finished. Converting...");
                var gitree = sourceNode.ConvertToGitree();
                Log.Verbose("- Converting finished. Saving to Gitree directory structure...");
                treeWriter.WriteItem(gitree, folder);
                Log.Debug("- Saved");
            }
        }

        private static ProjectConfigurationInfo CreateDestinationProjectConfig(DirectoryInfo sourceDir, DirectoryInfo destDir)
        {
            var configInfo = new ConvertedGitreeProjectConfigurationProvider().Create(sourceDir.FullName);
            var destFilepath = Path.Combine(destDir.FullName, Path.GetFileName(configInfo.Filepath));
            return configInfo.WithFilepath(destFilepath);
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

        private class ConvertedGitreeProjectConfigurationProvider : GitreeProjectConfigurationProvider
        {
            protected override ImmutableArray<SourceFolder> DefaultDirectoryReferences { get; } =
                ImmutableArray.Create(
                    new SourceFolder(SourceFolderKind.Catalogues, "src/catalogues"),
                    new SourceFolder(SourceFolderKind.Gamesystems, "src/gamesystems"));

            protected override ProjectConfiguration CreateDefaultCore(string path)
            {
                return base.CreateDefaultCore(path).WithSourceDirectories(DefaultDirectoryReferences);
            }
        }
    }
}
