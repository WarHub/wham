namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// This is an abstraction of a list-like lazy object provider.
    /// Similar to an <see cref="System.Collections.Generic.IReadOnlyList{T}"/>
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    internal interface IContainer<out TItem> where TItem : SourceNode
    {
        /// <summary>
        /// Gets the number of elements in the list.
        /// </summary>
        int SlotCount { get; }

        /// <summary>
        /// Gets an item at the specified index. This may cause computation if necessary.
        /// </summary>
        /// <param name="index">The index to retrieve item at.</param>
        /// <returns>Item at a given index.</returns>
        TItem GetNodeSlot(int index);
    }
}
