using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using WarHub.ArmouryModel.Source;
using WarHub.ArmouryModel.Source.BattleScribe;
using WarHub.ArmouryModel.Source.XmlFormat;
using Xunit;

namespace WarHub.ArmouryModel.CliTool.Tests.Publish
{
    public class VersionCheckTests
    {
        private const string WarningMessage = "BattleScribeVersion higher than supported";

        [Fact]
        public async Task When_file_version_is_higher_than_supported_then_logs_warning()
        {
            var output = new StringWriter();
            using var tmpContainer = TempDir.Create();
            var tmp = tmpContainer.TemporaryDir;
            var currentV = RootElement.GameSystem.Info().CurrentVersion;
            var higherV = currentV.WithMajor(currentV.Major + 1);
            var gst = NodeFactory.Gamesystem().WithBattleScribeVersion(higherV.BattleScribeString);
            using (var gstFileStream = File.Create(Path.Combine(tmp.FullName, "test.gst")))
            {
                gst.Serialize(gstFileStream);
            }

            await Program.CreateCommand().Parse($"publish -s \"{tmp}\" -o \"{tmp}\"").InvokeAsync(new() { Output = output });

            output.ToString()
                .Should().Contain(WarningMessage);
        }

        [Fact]
        public async Task When_file_version_is_lower_than_current_then_doesnt_log_warning()
        {
            var output = new StringWriter();
            using var tmpContainer = TempDir.Create();
            var tmp = tmpContainer.TemporaryDir;
            var currentV = RootElement.GameSystem.Info().CurrentVersion;
            var higherV = currentV.WithMajor(currentV.Major - 1);
            var gst = NodeFactory.Gamesystem().WithBattleScribeVersion(higherV.BattleScribeString);
            using (var gstFileStream = File.Create(Path.Combine(tmp.FullName, "test.gst")))
            {
                gst.Serialize(gstFileStream);
            }

            await Program.CreateCommand().Parse($"publish -s \"{tmp}\" -o \"{tmp}\"").InvokeAsync(new() { Output = output });

            output.ToString()
                .Should().NotContain(WarningMessage);
        }

        private sealed class TempDir : IDisposable
        {
            public TempDir(DirectoryInfo directory)
            {
                TemporaryDir = directory;
            }

            private bool disposedValue;

            public DirectoryInfo TemporaryDir { get; }

            public static TempDir Create()
            {
                var currentDir = Directory.GetCurrentDirectory();
                var tempPath = Path.Combine(currentDir, "testing", Path.GetRandomFileName());
                var tempDir = new DirectoryInfo(tempPath);
                tempDir.Create();
                return new TempDir(tempDir);
            }

            public void Dispose()
            {
                if (!disposedValue)
                {
                    TemporaryDir.Delete(recursive: true);
                    disposedValue = true;
                }
            }
        }
    }
}
