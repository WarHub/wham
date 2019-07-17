using System.CommandLine;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class ShowInfoCommand : CommandBase
    {
        public void Main(IConsole console)
        {
            console.Out.WriteLine($"Product: {ThisAssembly.AssemblyName} {ThisAssembly.AssemblyInformationalVersion}");
            console.Out.WriteLine($"Configuration: {ThisAssembly.AssemblyConfiguration}");
            console.Out.WriteLine($"File version: {ThisAssembly.AssemblyFileVersion}");
            console.Out.WriteLine($"Assembly version: {ThisAssembly.AssemblyVersion}");
        }
    }
}
