using CodeGeneration.Roslyn;
using System;
using System.Diagnostics;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [CodeGenerationAttribute(typeof(WhamNodeGenerator))]
    [Conditional("CodeGeneration")]
    public sealed class WhamNodeCoreAttribute : Attribute
    {
    }
}
