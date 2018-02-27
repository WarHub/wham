using System;
using PowerArgs;
using WarHub.ArmouryModel.Workspaces.JsonFolder;

namespace WarHub.ArmouryModel.CliTool.Commands.Convert
{
    public class ConvertJson : CommandBase
    {
        [ArgDescription("Specify to run continuously, watching source directory/file for changes.")]
        public bool Watch { get; set; }

        [ArgDescription("Directory in which to look for convertible files."), ArgExistingDirectory, ArgRequired]
        public string Source { get; set; }

        [ArgDescription("Directory into which to save conversion results"), ArgExistingDirectory, ArgRequired]
        public string Destination { get; set; }

        public void Main()
        {
            SetupLogger();
            Log.Debug("Source resolved to {Source}", Source);
            var workspace = JsonWorkspace.CreateFromDirectory(Source);
        }
    }
}
