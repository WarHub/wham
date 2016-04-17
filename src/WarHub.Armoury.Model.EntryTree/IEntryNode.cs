namespace WarHub.Armoury.Model.EntryTree
{
    public interface IEntryNode : INode
    {
        IEntry Entry { get; }
        IEntryLink Link { get; }
        EntryLinkPair EntryLinkPair { get; }
    }
}
