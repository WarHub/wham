using System;
using PowerArgs;
using WarHub.ArmouryModel.CliTool.Commands.Convert;

namespace WarHub.ArmouryModel.CliTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Args.InvokeAction<Program>();
        }

        [ArgActionMethod, ArgDescription("Converts BattleScribe XML files into JSON directory structure.")]
        public void ConvertXml(ConvertXml args) => args.Main();

        [ArgActionMethod, ArgDescription("Converts JSON directory structure into BattleScribe XML files.")]
        public void ConvertJson(ConvertJson args) => args.Main();
    }
}
