namespace WarHub.ArmouryModel.Source
{
    // this partial is separated to allow unit testing Source lib,
    // which requires SourceVisitor partial (other parts are generated)

    public partial class SourceNode
    {
        public abstract void Accept(SourceVisitor visitor);

        public abstract TResult Accept<TResult>(SourceVisitor<TResult> visitor);
    }
}
