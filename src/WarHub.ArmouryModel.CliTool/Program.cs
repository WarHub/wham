using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
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
            => await CreateParser().InvokeAsync(args);

        public static Parser CreateParser()
        {
            var infoOption = new Option("--info", "Display product information: name, configuration, various versions");
            return new CommandLineBuilder()
                .AddOption(infoOption)
                .UseMiddlewareOrdered(MiddlewareOrder.Preprocessing, async (ctx, next) =>
                {
                    if (ctx.ParseResult.HasOption(infoOption))
                    {
                        ShowInfoCommand.Run(ctx.Console);
                    }
                    else
                    {
                        await next(ctx);
                    }
                })
                .UseDefaults()
                .AddCommand(
                    new Command("convertxml", "[WIP] Converts BattleScribe XML files into Gitree directory structure.")
                    {
                        new Option(new[] { "-s", "--source" }, "Directory in which to look for XML files.")
                        {
                            Argument = new Argument<DirectoryInfo>(GetCurrentDirectoryInfo).ExistingOnly()
                        },
                        new Option(new[] { "-o", "--output" }, "Root directory in which to save Gitree files and folders.")
                        {
                            Argument = new Argument<DirectoryInfo>(GetCurrentDirectoryInfo).LegalFilePathsOnly()
                        },
                        CreateVerbosityOption()
                    }
                    .Hidden()
                    .Runs(typeof(ConvertXmlCommand).GetMethod(nameof(ConvertXmlCommand.Run))))
                .AddCommand(
                    new Command("convertgitree", "[WIP] Converts Gitree directory structure into BattleScribe XML files.")
                    {
                        new Option(new[] { "-s", "--source" }, "Root directory of Gitree to convert.")
                        {
                            Argument = new Argument<DirectoryInfo>(GetCurrentDirectoryInfo).ExistingOnly()
                        },
                        new Option(new[] { "-o", "--output" }, "Directory in which to save XML files.")
                        {
                            Argument = new Argument<DirectoryInfo>(GetCurrentDirectoryInfo).LegalFilePathsOnly()
                        },
                        CreateVerbosityOption()
                    }
                    .Hidden()
                    .Runs(typeof(ConvertGitreeCommand).GetMethod(nameof(ConvertGitreeCommand.Run))))
                .AddCommand(
                    new Command("publish", "Publishes given workspace in selected formats, by default a .bsr file.")
                    {
                        new Option(
                            new[] { "-a", "--artifacts" },
                            "Kinds of artifacts to publish to output (multiple values allowed):" +
                            " XML - uncompressed cat/gst XML files;" +
                            " ZIP - zipped catz/gstz XML files;" +
                            " INDEX - index.xml datafile index for hosting on the web;" +
                            " BSI - index.bsi zipped datafile index for hosting on the web;" +
                            " BSR - zipped cat/gst datafile container with index.")
                        {
                            Argument = new Argument<string>()
                            {
                                Arity = ArgumentArity.OneOrMore
                            }
                            .WithDefaultValue("bsr")
                            .FromAmong(PublishCommand.ArtifactNames)
                        },
                        new Option(new[] { "-s", "--source" }, "Directory in which to look for datafiles.")
                        {
                            Argument = new Argument<DirectoryInfo>(GetCurrentDirectoryInfo).ExistingOnly()
                        },
                        new Option(new[] { "-o", "--output" }, "Directory to save artifacts to.")
                        {
                            Argument = new Argument<DirectoryInfo>(GetCurrentDirectoryInfo).LegalFilePathsOnly()
                        },
                        new Option("--url", "Url of the index that gets included in indexes and bsr.")
                        {
                            Argument = new Argument<Uri>().RequireAbsoluteUrl()
                        },
                        new Option(
                            "--repo-name",
                            "Repository name used in published indexes (includes bsr)." +
                            " Default is name of the game system, or <source> folder name if no game system included.")
                        {
                            Argument = new Argument<string>()
                        },
                        new Option(
                            new[] { "-f", "--filename" },
                            "Filename (without extension) of the output artifacts. Default is <source> folder name.")
                        {
                            Argument = new Argument<string>().LegalFilePathsOnly()
                        },
                        new Option("--url-only-index", "Don't include data index entries in published indexes (doesn't impact bsr index).")
                        {
                            Argument = new Argument<bool>()
                        },
                        new Option("--additional-urls", "Additional urls of index/bsr included in published indexes (doesn't impact bsr index).")
                        {
                            Argument = new Argument<Uri[]>().RequireAbsoluteUrl()
                        },
                        CreateVerbosityOption()
                    }
                    .Runs(typeof(PublishCommand).GetMethod(nameof(PublishCommand.Run))))
                .Build();
        }

        internal static LogEventLevel GetLogLevel(string verbosity)
        {
            switch (verbosity)
            {
                case "q":
                case "quiet":
                    return LogEventLevel.Error;
                case "m":
                case "minimal":
                    return LogEventLevel.Warning;
                case "n":
                case "normal":
                    return LogEventLevel.Information;
                case "d":
                case "detailed":
                    return LogEventLevel.Debug;
                case "diag":
                case "diagnostic":
                case "Verbose": /* legacy option */
                    return LogEventLevel.Verbose;
                default:
                    return LogEventLevel.Information;
            }
        }

        private static DirectoryInfo GetCurrentDirectoryInfo() => new DirectoryInfo(".");

        private static Option CreateVerbosityOption() =>
            new Option(new[] { "-v", "--verbosity" }, "Set the verbosity level.")
            {
                Argument = new Argument<string>().FromAmong(verbosityLevels)
            };
    }
}
