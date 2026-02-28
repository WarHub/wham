using System;
using System.Globalization;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public abstract class CommandBase
    {
        protected ILogger Log { get; private set; } = Logger.None;

        public TextWriter Output { get; set; } = Console.Out;

        protected ILogger SetupLogger(string? verbosity)
        {
            var baseConfig = new LoggerConfiguration()
                .MinimumLevel.Is(Program.GetLogLevel(verbosity));

            var config = Output == Console.Out
                ? baseConfig.WriteTo.Console(theme: AnsiConsoleTheme.Code, formatProvider: CultureInfo.InvariantCulture)
                : baseConfig.WriteTo.Sink(new TextWriterSink(Output));

            return Log = config.CreateLogger();
        }

        private sealed class TextWriterSink : ILogEventSink
        {
            public TextWriterSink(TextWriter writer)
            {
                Writer = writer;
            }

            public TextWriter Writer { get; }

            public void Emit(LogEvent logEvent)
            {
                Writer.WriteLine(logEvent.RenderMessage(CultureInfo.InvariantCulture));
            }
        }
    }
}
