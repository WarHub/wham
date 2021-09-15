using System.CommandLine;
using System.CommandLine.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public abstract class CommandBase
    {
        protected ILogger Log { get; private set; } = Logger.None;

        public IConsole Console { get; set; } = new TestConsole();

        protected ILogger SetupLogger(string? verbosity)
        {
            var baseConfig = new LoggerConfiguration()
                .MinimumLevel.Is(Program.GetLogLevel(verbosity));

            var config = Console is SystemConsole ?
                baseConfig.WriteTo.Console(theme: AnsiConsoleTheme.Code)
                : baseConfig.WriteTo.Sink(new TestConsoleSink(Console));

            return Log = config.CreateLogger();
        }

        private class TestConsoleSink : ILogEventSink
        {
            public TestConsoleSink(IConsole console)
            {
                Console = console;
            }

            public IConsole Console { get; }

            public void Emit(LogEvent logEvent)
            {
                Console.Out.WriteLine(logEvent.RenderMessage());
            }
        }
    }
}
