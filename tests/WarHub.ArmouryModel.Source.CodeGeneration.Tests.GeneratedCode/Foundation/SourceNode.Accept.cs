using WarHub.ArmouryModel.Source.CodeGeneration.Tests.GeneratedCode;

namespace WarHub.ArmouryModel.Source
{
    public partial class SourceNode
    {
        public abstract void Accept(SourceVisitor visitor);

        public abstract TResult Accept<TResult>(SourceVisitor<TResult> visitor);
    }
}
