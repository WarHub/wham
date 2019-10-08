namespace WarHub.ArmouryModel.Source.Foundation
{
    public abstract class SourceReference
    {
        public abstract SourceTree Tree { get; }

        public abstract SourceNode GetNode();
    }
}
