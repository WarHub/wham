namespace WarHub.ArmouryModel.Source
{
    internal interface IContainer<out TItem> where TItem : SourceNode
    {
        int SlotCount { get; }
        TItem GetNodeSlot(int index);
    }
}
