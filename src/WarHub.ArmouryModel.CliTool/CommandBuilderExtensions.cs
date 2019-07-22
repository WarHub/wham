using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

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

        public static Argument WithValidator(this Argument argument, ValidateSymbol validator)
        {
            argument.AddValidator(validator);
            return argument;
        }

        public static Argument RequireAbsoluteUrl(this Argument argument)
        {
            return argument
                .WithValidator(result => result.Tokens.Select(UrlIsNotAbsoluteMessage)
                .FirstOrDefault());

            string UrlIsNotAbsoluteMessage(Token token)
            {
                try
                {
                    new Uri(token.Value, UriKind.Absolute);
                }
                catch (UriFormatException e)
                {
                    return $"{argument} has invalid value '{token.Value}'. {e.Message}";
                }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                catch (Exception)
                {
                    // anything else is of no concern
                }
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                return null;
            }
        }
    }
}
