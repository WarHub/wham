using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;

namespace WarHub.ArmouryModel.CliTool
{
    internal static class CommandExtensions
    {
        public static Command Runs(this Command command, MethodInfo methodInfo)
        {
            command.Handler = CommandHandler.Create(methodInfo);
            return command;
        }

        public static Command Runs<T>(this Command command, Action<T> action)
        {
            command.Handler = CommandHandler.Create(action);
            return command;
        }

        public static Command Runs<T>(this Command command, Func<T, int> func)
        {
            command.Handler = CommandHandler.Create(func);
            return command;
        }
    }
}
