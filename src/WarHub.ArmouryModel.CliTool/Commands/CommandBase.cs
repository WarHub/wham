using System;
using PowerArgs;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    [ArgEnforceCase]
    public abstract class CommandBase
    {
        protected CliGlobalCommand Global { get; private set; }

        protected Logger Log { get; private set; }

        protected bool WaitForKey => Global?.WaitForKey ?? false;

        protected LogEventLevel Verbosity => Global?.Verbosity ?? LogEventLevel.Information;

        public virtual void Main(CliGlobalCommand global)
        {
            Global = global;
            SetupLogger();
            Log.Information($"{ThisAssembly.AssemblyName} {ThisAssembly.AssemblyInformationalVersion}");
            MainCore();
            WaitForEnter();
        }

        protected virtual void MainCore()
        {
        }

        protected virtual void SetupLogger()
        {
            Log = new LoggerConfiguration()
                .MinimumLevel.Is(Verbosity)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
        }

        protected virtual void WaitForEnter()
        {
            if (WaitForKey)
            {
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
            }
        }
    }
}
