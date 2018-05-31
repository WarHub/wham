using PowerArgs;
using Serilog.Events;
using WarHub.ArmouryModel.CliTool.Commands;

namespace WarHub.ArmouryModel.CliTool
{
    [ArgProductName(ThisAssembly.AssemblyName), ArgProductVersion(ThisAssembly.AssemblyInformationalVersion)]
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling), ArgEnforceCase]
    public class CliGlobalCommand
    {
        [HelpHook]
        [ArgShortcut("?"), ArgShortcut("h"), ArgShortcut("help")]
        public bool ShowHelp { get; set; }

        [ArgShortcut("w")]
        [ArgDescription("Set if the tool should wait until any key is pressed after finishing it's work.")]
        public bool WaitForKey { get; set; } = System.Diagnostics.Debugger.IsAttached;

        [ArgShortcut("v")]
        [ArgDescription("Set verbosity of output"), ArgDefaultValue(LogEventLevel.Information)]
        public LogEventLevel Verbosity { get; set; } = LogEventLevel.Information;

        [ArgActionMethod, ArgShortcut("version"), ArgShortcut(ArgShortcutPolicy.ShortcutsOnly)]
        [ArgDescription("Show detailed version information and exit.")]
        public void ShowVersion(ShowVersionCommand cmd) => cmd.Main(this);

        [ArgActionMethod, ArgShortcut("convertxml")]
        [ArgDescription("Converts BattleScribe XML files into JSON directory structure.")]
        public void ConvertXml(ConvertXmlCommand cmd) => cmd.Main(this);

        [ArgActionMethod, ArgShortcut("convertjson")]
        [ArgDescription("Converts JSON directory structure into BattleScribe XML files.")]
        public void ConvertJson(ConvertJsonCommand cmd) => cmd.Main(this);

        [ArgActionMethod, ArgShortcut("publish")]
        [ArgDescription("Publishes given workspace into multiple available formats, e.g. .bsr file.")]
        public void Publish(PublishCommand cmd) => cmd.Main(this);
    }
}
