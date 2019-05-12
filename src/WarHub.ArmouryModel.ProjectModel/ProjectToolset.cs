using System;

namespace WarHub.ArmouryModel.ProjectModel
{
    public class ProjectToolset
    {
        public static string Version { get; } = new Version(ThisAssembly.AssemblyVersion).ToString(2);

        public static string BattleScribeFormatVersion { get; } = "2.02";

        public static string BattleScribeDataIndexFormatVersion { get; } = "1.13b";
    }
}
