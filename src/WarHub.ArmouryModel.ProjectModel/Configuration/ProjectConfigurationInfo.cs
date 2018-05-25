using System;
using System.Collections.Generic;
using System.Text;

namespace WarHub.ArmouryModel.ProjectModel
{
    [Record]
    public partial class ProjectConfigurationInfo
    {
        public string Filepath { get; }

        public ProjectConfiguration Configuration { get; }
    }
}
