using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading.Tasks;
using Serilog.Events;
using WarHub.ArmouryModel.CliTool.Commands;

namespace WarHub.ArmouryModel.CliTool
{
    public static class Program
    {
        private static readonly string[] verbosityLevels = new[] { "q", "quiet", "m", "minimal", "n", "normal", "d", "detailed", "diag", "diagnostic" };

        internal static async Task<int> Main(string[] args)
            => await CreateCommand().Parse(args).InvokeAsync();

        public static RootCommand CreateCommand()
        {
            var infoOption = new Option<bool>("--info")
            {
                Description = "Display product information: name, configuration, various versions"
            };

            var root = new RootCommand();
            root.Options.Add(infoOption);
            root.SetAction(result =>
            {
                if (result.GetValue(infoOption))
                {
                    ShowInfoCommand.Run(result.InvocationConfiguration.Output);
                }
            });

            root.Subcommands.Add(CreateConvertXmlCommand());
            root.Subcommands.Add(CreateConvertGitreeCommand());
            root.Subcommands.Add(CreatePublishCommand());

            return root;
        }

        private static Command CreateConvertXmlCommand()
        {
            var sourceOption = new Option<DirectoryInfo>("--source", "-s")
            {
                Description = "Directory in which to look for XML files.",
                DefaultValueFactory = _ => new DirectoryInfo(".")
            };
            var outputOption = new Option<DirectoryInfo>("--output", "-o")
            {
                Description = "Root directory in which to save Gitree files and folders.",
                DefaultValueFactory = _ => new DirectoryInfo(".")
            };
            var verbosityOption = CreateVerbosityOption();

            var command = new Command("convertxml", "[WIP] Converts BattleScribe XML files into Gitree directory structure.")
            {
                sourceOption, outputOption, verbosityOption
            };
            command.Hidden = true;
            command.SetAction(async (parseResult, ct) =>
            {
                var cmd = new ConvertXmlCommand { Output = parseResult.InvocationConfiguration.Output };
                await cmd.RunAsync(
                    parseResult.GetValue(sourceOption)!,
                    parseResult.GetValue(outputOption)!,
                    parseResult.GetValue(verbosityOption)!);
            });
            return command;
        }

        private static Command CreateConvertGitreeCommand()
        {
            var sourceOption = new Option<DirectoryInfo>("--source", "-s")
            {
                Description = "Root directory of Gitree to convert.",
                DefaultValueFactory = _ => new DirectoryInfo(".")
            };
            var outputOption = new Option<DirectoryInfo>("--output", "-o")
            {
                Description = "Directory in which to save XML files.",
                DefaultValueFactory = _ => new DirectoryInfo(".")
            };
            var verbosityOption = CreateVerbosityOption();

            var command = new Command("convertgitree", "[WIP] Converts Gitree directory structure into BattleScribe XML files.")
            {
                sourceOption, outputOption, verbosityOption
            };
            command.Hidden = true;
            command.SetAction(async (parseResult, ct) =>
            {
                var cmd = new ConvertGitreeCommand { Output = parseResult.InvocationConfiguration.Output };
                await cmd.RunAsync(
                    parseResult.GetValue(sourceOption)!,
                    parseResult.GetValue(outputOption)!,
                    parseResult.GetValue(verbosityOption)!);
            });
            return command;
        }

        private static Command CreatePublishCommand()
        {
            var artifactsOption = new Option<string[]>("--artifacts", "-a")
            {
                Description = "Kinds of artifacts to publish to output (multiple values allowed):" +
                    " XML - uncompressed cat/gst XML files;" +
                    " ZIP - zipped catz/gstz XML files;" +
                    " INDEX - index.xml datafile index for hosting on the web;" +
                    " BSI - index.bsi zipped datafile index for hosting on the web;" +
                    " BSR - zipped cat/gst datafile container with index.",
                Arity = ArgumentArity.OneOrMore,
                AllowMultipleArgumentsPerToken = true,
                DefaultValueFactory = _ => new[] { "bsr" }
            };
            artifactsOption.AcceptOnlyFromAmong(PublishCommand.ArtifactNames);

            var sourceOption = new Option<DirectoryInfo>("--source", "-s")
            {
                Description = "Directory in which to look for datafiles.",
                DefaultValueFactory = _ => new DirectoryInfo(".")
            };
            var outputOption = new Option<DirectoryInfo>("--output", "-o")
            {
                Description = "Directory to save artifacts to.",
                DefaultValueFactory = _ => new DirectoryInfo(".")
            };
            var urlOption = new Option<Uri>("--url")
            {
                Description = "Url of the index that gets included in indexes and bsr."
            };
            urlOption.AddAbsoluteUriValidator();
            var repoNameOption = new Option<string>("--repo-name")
            {
                Description = "Repository name used in published indexes (includes bsr)." +
                    " Default is name of the game system, or <source> folder name if no game system included."
            };
            var filenameOption = new Option<string>("--filename", "-f")
            {
                Description = "Filename (without extension) of the output artifacts. Default is <source> folder name."
            };
            var urlOnlyIndexOption = new Option<bool>("--url-only-index")
            {
                Description = "Don't include data index entries in published indexes (doesn't impact bsr index)."
            };
            var additionalUrlsOption = new Option<Uri[]>("--additional-urls")
            {
                Description = "Additional urls of index/bsr included in published indexes (doesn't impact bsr index)."
            };
            additionalUrlsOption.AddAbsoluteUriValidator();
            var verbosityOption = CreateVerbosityOption();

            var command = new Command("publish", "Publishes given workspace in selected formats, by default a .bsr file.")
            {
                artifactsOption, sourceOption, outputOption, urlOption,
                repoNameOption, filenameOption, urlOnlyIndexOption,
                additionalUrlsOption, verbosityOption
            };
            command.SetAction(async (parseResult, ct) =>
            {
                var cmd = new PublishCommand { Output = parseResult.InvocationConfiguration.Output };
                await cmd.RunAsync(
                    parseResult.GetValue(artifactsOption) ?? Array.Empty<string>(),
                    parseResult.GetValue(sourceOption)!,
                    parseResult.GetValue(outputOption)!,
                    parseResult.GetValue(urlOption),
                    parseResult.GetValue(additionalUrlsOption) ?? Array.Empty<Uri>(),
                    parseResult.GetValue(urlOnlyIndexOption),
                    parseResult.GetValue(repoNameOption),
                    parseResult.GetValue(filenameOption),
                    parseResult.GetValue(verbosityOption));
            });
            return command;
        }

        internal static LogEventLevel GetLogLevel(string? verbosity) => verbosity switch
        {
            "q" or "quiet" => LogEventLevel.Error,
            "m" or "minimal" => LogEventLevel.Warning,
            "n" or "normal" => LogEventLevel.Information,
            "d" or "detailed" => LogEventLevel.Debug,
            "diag" or "diagnostic" or "Verbose" => LogEventLevel.Verbose,
            _ => LogEventLevel.Information,
        };

        private static Option<string> CreateVerbosityOption()
        {
            var option = new Option<string>("--verbosity", "-v")
            {
                Description = "Set the verbosity level."
            };
            option.AcceptOnlyFromAmong(verbosityLevels);
            return option;
        }
    }
}
