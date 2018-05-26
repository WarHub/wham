using System;
using System.Collections.Generic;
using System.Text;
using PowerArgs;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace WarHub.ArmouryModel.CliTool.Commands
{
    [ArgEnforceCase]
    public class CommandBase
    {
        protected Logger Log { get; private set; }

        [ArgShortcut("w")]
        [ArgDescription("Set if the tool should wait until any key is pressed after finishing it's work.")]
        public bool WaitForKey { get; set; } = System.Diagnostics.Debugger.IsAttached;

        [ArgShortcut("v")]
        [ArgDescription("Set verbosity of output"), ArgDefaultValue(LogEventLevel.Information)]
        public LogEventLevel Verbosity { get; set; } = LogEventLevel.Information;

        public void Main()
        {
            SetupLogger();
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
