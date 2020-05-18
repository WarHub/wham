namespace WarHub.ArmouryModel.Source.Foundation
{
    internal abstract class SourceReference
    {
        public abstract SourceTree Tree { get; }

        public abstract SourceNode GetNode();
    }
}
