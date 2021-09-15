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

        public static TOption WithArity<TOption>(this TOption option, IArgumentArity arity) where TOption : Option
        {
            option.Arity = arity;
            return option;
        }

        public static Option<T> WithDefaultValue<T>(this Option<T> option, T defaultValue)
        {
            option.SetDefaultValue(defaultValue);
            return option;
        }

        public static TOption RequireAbsoluteUrl<TOption>(this TOption option) where TOption : Option
        {
            option.AddValidator(symbol =>
                    (from token in symbol.Tokens
                     let value = token.Value
                     where !Uri.TryCreate(value, UriKind.Absolute, out var _)
                     select $"Invalid URI '{value}'.")
                    .FirstOrDefault());
            return option;
        }
    }
}
