using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.JsonInfrastructure;
using WarHub.ArmouryModel.ProjectModel;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.JsonFolder;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class ConvertXmlCommand : CommandBase
    {
        [ArgShortcut("s")]
        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory]
        [ArgDefaultValue(".")]
        public string Source { get; set; }

        [ArgShortcut("d")]
        [ArgDescription("Directory into which to save conversion results.")]
        [ArgDefaultValue(".")]
        public string Destination { get; set; }

        protected override void MainCore()
        {
            var sourceDir = ResolveSourceDir();
            var destDir = ResolveDestinationDir();
            var configInfo = CreateDestinationProjectConfig(sourceDir, destDir);
            var workspace = CreateXmlWorkspace(sourceDir);
            configInfo.WriteFile();
            Log.Information("Project configuration saved as {ConfigFile}", configInfo.Filepath);

            ConvertFiles(configInfo, workspace);
        }

        private void ConvertFiles(ProjectConfigurationInfo configInfo, XmlWorkspace workspace)
        {
            var treeConverter = new SourceNodeToJsonTreeConverter();
            var treeWriter = new JsonTreeWriter();
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
                var jsonTree = treeConverter.Visit(sourceNode);
                Log.Verbose("- Converting finished. Saving to JSON directory structure...");
                treeWriter.WriteItem(jsonTree, folder);
                Log.Debug("- Saved");
            }
        }

        private class XmlToJsonConverter
        {
            private SourceNodeToJsonTreeConverter treeConverter { get; }

            private JsonTreeWriter treeWriter { get; }

            private ImmutableDictionary<XmlDocumentKind, string> pathsForDocumentKinds { get; }

            public ProjectConfigurationInfo ConfigInfo { get; }

            public XmlWorkspace Workspace { get; }

            public XmlToJsonConverter(ProjectConfigurationInfo configInfo, XmlWorkspace workspace)
            {
                pathsForDocumentKinds = new Dictionary<XmlDocumentKind, string>
                {
                    [XmlDocumentKind.Gamesystem] = configInfo.GetFullPath(SourceKind.Gamesystem),
                    [XmlDocumentKind.Catalogue] = configInfo.GetFullPath(SourceKind.Catalogue)
                }.ToImmutableDictionary();
                ConfigInfo = configInfo;
                Workspace = workspace;
            }
            
        }

        private static ProjectConfigurationInfo CreateDestinationProjectConfig(DirectoryInfo sourceDir, DirectoryInfo destDir)
        {
            var configInfo = new ConvertedJsonProjectConfigurationProvider().Create(sourceDir.FullName);
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

        private DirectoryInfo ResolveDestinationDir()
        {
            var destDir = new DirectoryInfo(Destination ?? ".");
            destDir.Create();
            Log.Debug("Destination directory resolved to {Destination}", destDir);
            return destDir;
        }

        private DirectoryInfo ResolveSourceDir()
        {
            var sourceDir = Source != null ? new DirectoryInfo(Source) : new DirectoryInfo(".");
            Log.Debug("Source resolved to {Source}", sourceDir);
            return sourceDir;
        }

        private class ConvertedJsonProjectConfigurationProvider : JsonFolderProjectConfigurationProvider
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
