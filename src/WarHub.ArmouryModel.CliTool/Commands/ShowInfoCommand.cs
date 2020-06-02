using System.CommandLine;
using System.CommandLine.IO;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public static class ShowInfoCommand
    {
        public static void Run(IConsole console)
        {
            console.Out.WriteLine($"Product: {ThisAssembly.AssemblyName} {ThisAssembly.AssemblyInformationalVersion}");
            console.Out.WriteLine($"Configuration: {ThisAssembly.AssemblyConfiguration}");
            console.Out.WriteLine($"File version: {ThisAssembly.AssemblyFileVersion}");
            console.Out.WriteLine($"Assembly version: {ThisAssembly.AssemblyVersion}");
        }
    }
}
