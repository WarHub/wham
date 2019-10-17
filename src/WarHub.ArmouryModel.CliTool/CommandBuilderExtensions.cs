using System;
using System.CommandLine;
using System.CommandLine.Builder;
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

        public static CommandLineBuilder UseMiddlewareOrdered(this CommandLineBuilder builder, int order, InvocationMiddleware middleware)
        {
            addMiddlewareLazy.Value(builder, middleware, order);
            return builder;
        }

        // hack: surfacing internal method as an action. Lazy-initialized.
        private static readonly Lazy<Action<CommandLineBuilder, InvocationMiddleware, int>> addMiddlewareLazy = new Lazy<Action<CommandLineBuilder, InvocationMiddleware, int>>(() =>
        {
            var methodInfo = typeof(CommandLineBuilder).GetMethod("AddMiddleware", BindingFlags.Instance | BindingFlags.NonPublic);
            return (CommandLineBuilder builder, InvocationMiddleware middleware, int order) => methodInfo.Invoke(builder, new object[] { middleware, order });
        });

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

    // from https://github.com/dotnet/command-line-api/blob/0427856988a6b2e92db70f941e885bb87020488f/src/System.CommandLine/Builder/CommandLineBuilder.cs
    internal static class MiddlewareOrder
    {
        public const int ProcessExit = int.MinValue;
        public const int ExceptionHandler = ProcessExit + 100;
        public const int Configuration = ExceptionHandler + 100;
        public const int Preprocessing = Configuration + 100;
        public const int AfterPreprocessing = Preprocessing + 100;
        public const int Middle = 0;
    }
}
