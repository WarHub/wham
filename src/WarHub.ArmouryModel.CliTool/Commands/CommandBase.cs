using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    public abstract class CommandBase
    {
        protected Logger Log { get; private set; }

        protected Logger SetupLogger(string verbosity)
        {
            return Log = new LoggerConfiguration()
                .MinimumLevel.Is(Program.GetLogLevel(verbosity))
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
        }
    }
}
