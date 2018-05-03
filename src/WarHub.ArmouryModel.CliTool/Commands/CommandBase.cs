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
    public class CommandBase
    {
        protected Logger Log { get; private set; }

        [ArgDescription("Set verbosity of output"), ArgDefaultValue(LogEventLevel.Information)]
        public LogEventLevel Verbosity { get; set; } = LogEventLevel.Information;

        [ArgDescription("Set if the tool should wait until any key is pressed after finishing it's work.")]
        public bool WaitForKey { get; set; }

        public void SetupLogger()
        {
            Log = new LoggerConfiguration()
                .MinimumLevel.Is(Verbosity)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
        }

        protected void WaitForReadKey()
        {
            if (WaitForKey)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
