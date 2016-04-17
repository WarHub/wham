namespace WarHub.Armoury.Model.EntryTree
{
    public interface IGroupNode : INode
    {
        IGroup Group { get; }
        GroupLinkPair GroupLinkPair { get; }
        IGroupLink Link { get; }
    }
}
