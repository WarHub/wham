using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;
using WarHub.ArmouryModel.Source.CodeGeneration;

namespace WarHub.ArmouryModel.Source
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [CodeGenerationAttribute(typeof(WhamNodeGenerator))]
    [Conditional("CodeGeneration")]
    public sealed class WhamNodeCoreAttribute : Attribute
    {
    }
}
