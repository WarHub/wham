using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace WarHub.ArmouryModel.Source
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [CodeGenerationAttribute("WarHub.ArmouryModel.Source.CodeGeneration.WhamNodeGenerator, WarHub.ArmouryModel.Source.CodeGeneration")]
    [Conditional("CodeGeneration")]
    public sealed class WhamNodeCoreAttribute : Attribute
    {
    }
}
