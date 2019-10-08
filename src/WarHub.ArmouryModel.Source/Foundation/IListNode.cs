namespace WarHub.ArmouryModel.Source
{
    public interface IListNode
    {
        SourceKind Kind { get; }

        SourceKind ElementKind { get; }

        NodeList<SourceNode> NodeList { get; }
    }
}
