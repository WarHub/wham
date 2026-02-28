using System;
using System.CommandLine;
using System.IO;
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
            var output = new StringWriter();

            await Program.CreateCommand().Parse("--info").InvokeAsync(new() { Output = output });
            var result = output.ToString();

            result.Should()
                .StartWith("Product: wham " + ThisAssembly.AssemblyInformationalVersion)
                .And
                .EndWith("Assembly version: " + ThisAssembly.AssemblyVersion + Environment.NewLine);
        }
    }
}
