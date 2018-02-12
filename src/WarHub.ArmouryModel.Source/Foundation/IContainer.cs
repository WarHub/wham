namespace WarHub.ArmouryModel.Source
{
    public interface IContainer<out TItem> where TItem : SourceNode
    {
        int SlotCount { get; }
        TItem GetNodeSlot(int index);
    }
}