using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarHub.ArmouryModel.ProjectSystem
{
    public static class ProjectConfigurationExtensions
    {
        public static DirectoryReference GetRefForKind(this ProjectConfiguration config, DirectoryReferenceKind kind)
        {
            return config.SourceDirectories.Single(dir => dir.Kind == kind);
        }
    }
}
