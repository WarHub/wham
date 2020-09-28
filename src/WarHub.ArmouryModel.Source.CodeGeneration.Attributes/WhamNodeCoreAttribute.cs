using System;
using System.Diagnostics;

namespace WarHub.ArmouryModel.Source
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    public sealed class WhamNodeCoreAttribute : Attribute
    {
    }
}
