using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WarHub.ArmouryModel.CliTool
{
    internal static class CommandBuilderExtensions
    {
        public static T Hidden<T>(this T symbol) where T : Symbol
        {
            symbol.IsHidden = true;
            return symbol;
        }

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

        public static Command Runs<T>(this Command command, Func<T, Task> action)
        {
            command.Handler = CommandHandler.Create(action);
            return command;
        }

        public static Command Runs<T>(this Command command, Func<T, int> func)
        {
            command.Handler = CommandHandler.Create(func);
            return command;
        }

        public static Argument WithDefaultValue(this Argument argument, object defaultValue)
        {
            argument.SetDefaultValue(defaultValue);
            return argument;
        }

        public static Argument WithValidator(this Argument argument, ValidateSymbol<ArgumentResult> validator)
        {
            argument.AddValidator(validator);
            return argument;
        }

        public static Argument RequireAbsoluteUrl(this Argument argument)
        {
            return argument
                .WithValidator(symbol =>
                    (from token in symbol.Tokens
                     let value = token.Value
                     where !Uri.TryCreate(value, UriKind.Absolute, out var _)
                     select $"Invalid URI '{value}'.")
                    .FirstOrDefault());
        }
    }
}
