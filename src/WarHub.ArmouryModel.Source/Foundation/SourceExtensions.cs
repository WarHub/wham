using System;

namespace WarHub.ArmouryModel.Source
{
    public static class SourceExtensions
    {
        public static bool IsKind(this SourceNode @this, SourceKind kind) => @this.Kind == kind;
    }
}
