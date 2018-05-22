using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.JsonInfrastructure;
using WarHub.ArmouryModel.ProjectSystem;
using WarHub.ArmouryModel.Workspaces.BattleScribe;
using WarHub.ArmouryModel.Workspaces.JsonFolder;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class PublishCommand : CommandBase
    {
        [ArgDescription("Directory in which to look for project file or datafiles.")]
        public string Source { get; set; }

        [ArgDescription("File or directory to save artifacts to.")]
        public string Destination { get; set; }

        public void Main()
        {
            SetupLogger();


        }
    }
}
