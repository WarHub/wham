using System.Diagnostics.CodeAnalysis;

namespace WarHub.ArmouryModel.Source
{
    // this partial is separated to allow unit testing Source lib,
    // which requires SourceVisitor partial (other parts are generated)

    public partial class SourceNode
    {
        public abstract void Accept(SourceVisitor visitor);

        [return: MaybeNull]
        public abstract TResult Accept<TResult>(SourceVisitor<TResult> visitor);
    }
}
