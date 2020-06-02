using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace WarHub.ArmouryModel.CliTool.Tests
{
    public class ShowInfoTests
    {
        [Fact]
        public async Task When_version_command_then_correct_versions_printed()
        {
            var console = new TestConsole();

            await Program.CreateParser().InvokeAsync("--info", console);
            var output = console.Out.ToString();

            output.Should()
                .StartWith("Product: wham " + ThisAssembly.AssemblyInformationalVersion)
                .And
                .EndWith("Assembly version: " + ThisAssembly.AssemblyVersion + Environment.NewLine);
        }
    }
}
