using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
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
            => await CreateParser().InvokeAsync(args);

        public static Parser CreateParser()
        {
            var infoOption = new Option("--info", "Display product information: name, configuration, various versions");
            return new CommandLineBuilder()
                .AddOption(infoOption)
                .UseMiddleware(async (ctx, next) =>
                {
                    if (ctx.ParseResult.HasOption(infoOption))
                    {
                        ShowInfoCommand.Run(ctx.Console);
                    }
                    else
                    {
                        await next(ctx);
                    }
                }, MiddlewareOrder.Configuration)
                .UseDefaults()
                .AddCommand(
                    new Command("convertxml", "[WIP] Converts BattleScribe XML files into Gitree directory structure.")
                    {
                        new Option<DirectoryInfo>(
                            new[] { "-s", "--source" },
                            GetCurrentDirectoryInfo,
                            "Directory in which to look for XML files.")
                            .ExistingOnly(),
                        new Option<DirectoryInfo>(
                            new[] { "-o", "--output" },
                            GetCurrentDirectoryInfo,
                            "Root directory in which to save Gitree files and folders.")
                            .LegalFilePathsOnly(),
                        CreateVerbosityOption(),
                    }
                    .Hidden()
                    .Runs(typeof(ConvertXmlCommand).GetMethod(nameof(ConvertXmlCommand.RunAsync))!))
                .AddCommand(
                    new Command("convertgitree", "[WIP] Converts Gitree directory structure into BattleScribe XML files.")
                    {
                        new Option<DirectoryInfo>(
                            new[] { "-s", "--source" },
                            GetCurrentDirectoryInfo,
                            "Root directory of Gitree to convert.")
                            .ExistingOnly(),
                        new Option<DirectoryInfo>(
                            new[] { "-o", "--output" },
                            GetCurrentDirectoryInfo,
                            "Directory in which to save XML files.")
                            .LegalFilePathsOnly(),
                        CreateVerbosityOption(),
                    }
                    .Hidden()
                    .Runs(typeof(ConvertGitreeCommand).GetMethod(nameof(ConvertGitreeCommand.RunAsync))!))
                .AddCommand(
                    new Command("publish", "Publishes given workspace in selected formats, by default a .bsr file.")
                    {
                        new Option<string>(
                            new[] { "-a", "--artifacts" },
                            "Kinds of artifacts to publish to output (multiple values allowed):" +
                            " XML - uncompressed cat/gst XML files;" +
                            " ZIP - zipped catz/gstz XML files;" +
                            " INDEX - index.xml datafile index for hosting on the web;" +
                            " BSI - index.bsi zipped datafile index for hosting on the web;" +
                            " BSR - zipped cat/gst datafile container with index.")
                            .WithArity(ArgumentArity.OneOrMore)
                            .WithDefaultValue("bsr")
                            .FromAmong(PublishCommand.ArtifactNames),
                        new Option<DirectoryInfo>(
                            new[] { "-s", "--source" },
                            GetCurrentDirectoryInfo,
                            "Directory in which to look for datafiles.")
                            .ExistingOnly(),
                        new Option<DirectoryInfo>(
                            new[] { "-o", "--output" },
                            GetCurrentDirectoryInfo,
                            "Directory to save artifacts to.")
                            .LegalFilePathsOnly(),
                        new Option<Uri>(
                            "--url",
                            "Url of the index that gets included in indexes and bsr.")
                            .RequireAbsoluteUrl(),
                        new Option<string>(
                            "--repo-name",
                            "Repository name used in published indexes (includes bsr)." +
                            " Default is name of the game system, or <source> folder name if no game system included."),
                        new Option<string>(
                            new[] { "-f", "--filename" },
                            "Filename (without extension) of the output artifacts. Default is <source> folder name.")
                            .LegalFilePathsOnly(),
                        new Option<bool>(
                            "--url-only-index",
                            "Don't include data index entries in published indexes (doesn't impact bsr index)."),
                        new Option<Uri[]>(
                            "--additional-urls",
                            "Additional urls of index/bsr included in published indexes (doesn't impact bsr index).")
                            .RequireAbsoluteUrl(),
                        CreateVerbosityOption(),
                    }
                    .Runs(typeof(PublishCommand).GetMethod(nameof(PublishCommand.RunAsync))!))
                .Build();
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

        private static DirectoryInfo GetCurrentDirectoryInfo() => new(".");

        private static Option CreateVerbosityOption() =>
            new Option<string>(
                new[] { "-v", "--verbosity" },
                "Set the verbosity level.")
                .FromAmong(verbosityLevels);
    }
}
