using System;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public class ShowVersionCommand : CommandBase
    {
        public override void Main(CliGlobalCommand global)
        {
            Console.WriteLine($"Product: {ThisAssembly.AssemblyName} {ThisAssembly.AssemblyInformationalVersion}");
            Console.WriteLine($"Configuration: {ThisAssembly.AssemblyConfiguration}");
            Console.WriteLine($"File version: {ThisAssembly.AssemblyFileVersion}");
            Console.WriteLine($"Assembly version: {ThisAssembly.AssemblyVersion}");
        }
    }
}
