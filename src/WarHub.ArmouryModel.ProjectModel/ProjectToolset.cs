using System;

namespace WarHub.ArmouryModel.ProjectModel
{
    public static class ProjectToolset
    {
        public static string Version { get; } = new Version(ThisAssembly.AssemblyVersion).ToString(2);
    }
}
