using System.IO;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public static class ShowInfoCommand
    {
        public static void Run(TextWriter output)
        {
            output.WriteLine($"Product: {ThisAssembly.AssemblyName} {ThisAssembly.AssemblyInformationalVersion}");
            output.WriteLine($"Configuration: {ThisAssembly.AssemblyConfiguration}");
            output.WriteLine($"File version: {ThisAssembly.AssemblyFileVersion}");
            output.WriteLine($"Assembly version: {ThisAssembly.AssemblyVersion}");
        }
    }
}
