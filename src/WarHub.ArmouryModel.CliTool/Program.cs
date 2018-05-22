using System;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.Commands;

namespace WarHub.ArmouryModel.CliTool
{
    public class Program
    {
        static void Main(string[] args)
        {
            Args.InvokeAction<CliActions>(args);
        }
    }

    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class CliActions
    {
        [ArgActionMethod, ArgDescription("Converts BattleScribe XML files into JSON directory structure.")]
        public void ConvertXml(ConvertXmlCommand args) => args.Main();

        [ArgActionMethod, ArgDescription("Converts JSON directory structure into BattleScribe XML files.")]
        public void ConvertJson(ConvertJsonCommand args) => args.Main();

        [ArgActionMethod, ArgDescription("Publishes given workspace into multiple available formats, e.g. .bsr file.")]
        public void Publish(PublishCommand args) => args.Main();
    }
}
