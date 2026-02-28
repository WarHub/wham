using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace WarHub.ArmouryModel.CliTool
{
    internal static class CommandBuilderExtensions
    {
        public static void AddAbsoluteUriValidator(this Option option)
        {
            option.Validators.Add(result =>
            {
                foreach (var token in result.Tokens)
                {
                    if (!Uri.TryCreate(token.Value, UriKind.Absolute, out _))
                    {
                        result.AddError($"Invalid URI '{token.Value}'.");
                    }
                }
            });
        }
    }
}
