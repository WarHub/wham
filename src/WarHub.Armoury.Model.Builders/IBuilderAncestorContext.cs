namespace WarHub.Armoury.Model.Builders
{
    public interface IBuilderAncestorContext
    {
        ICategoryBuilder CategoryBuilder { get; }
        IEntryBuilder EntryBuilder { get; }
        IForceBuilder ForceBuilder { get; }
        IGroupBuilder GroupBuilder { get; }
        IRosterBuilder RosterBuilder { get; }
        ISelectionBuilder SelectionBuilder { get; }
        IBuilderAncestorContext AppendedWith(IForceBuilder builder);
        IBuilderAncestorContext AppendedWith(ICategoryBuilder builder);
        IBuilderAncestorContext AppendedWith(IEntryBuilder builder);
        IBuilderAncestorContext AppendedWith(IGroupBuilder builder);
        IBuilderAncestorContext AppendedWith(ISelectionBuilder builder);
    }
}
