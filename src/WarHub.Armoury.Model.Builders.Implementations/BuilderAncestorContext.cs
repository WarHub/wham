namespace WarHub.Armoury.Model.Builders.Implementations
{
    using System;

    public class BuilderAncestorContext : IBuilderAncestorContext
    {
        private BuilderAncestorContext(IRosterBuilder rosterBuilder, IForceBuilder forceBuilder,
            ICategoryBuilder categoryBuilder, IEntryBuilder entryBuilder, IGroupBuilder groupBuilder,
            ISelectionBuilder selectionBuilder)
        {
            RosterBuilder = rosterBuilder;
            ForceBuilder = forceBuilder;
            CategoryBuilder = categoryBuilder;
            EntryBuilder = entryBuilder;
            GroupBuilder = groupBuilder;
            SelectionBuilder = selectionBuilder;
        }

        public IBuilderAncestorContext AppendedWith(IForceBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return new BuilderAncestorContext(RosterBuilder, builder, null, null, null, null);
        }

        public IBuilderAncestorContext AppendedWith(ICategoryBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (ForceBuilder == null) throw new InvalidOperationException($"Set {ForceBuilder} first!");
            return new BuilderAncestorContext(RosterBuilder, ForceBuilder, builder, null, null, null);
        }

        public IBuilderAncestorContext AppendedWith(IEntryBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (ForceBuilder == null) throw new InvalidOperationException($"Set {ForceBuilder} first!");
            if (CategoryBuilder == null) throw new InvalidOperationException($"Set {CategoryBuilder} first!");
            return new BuilderAncestorContext(RosterBuilder, ForceBuilder, CategoryBuilder, builder, GroupBuilder,
                SelectionBuilder);
        }

        public IBuilderAncestorContext AppendedWith(IGroupBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (ForceBuilder == null) throw new InvalidOperationException($"Set {ForceBuilder} first!");
            if (CategoryBuilder == null) throw new InvalidOperationException($"Set {CategoryBuilder} first!");
            return new BuilderAncestorContext(RosterBuilder, ForceBuilder, CategoryBuilder, EntryBuilder, builder,
                SelectionBuilder);
        }

        public IBuilderAncestorContext AppendedWith(ISelectionBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (ForceBuilder == null) throw new InvalidOperationException($"Set {ForceBuilder} first!");
            if (CategoryBuilder == null) throw new InvalidOperationException($"Set {CategoryBuilder} first!");
            if (CategoryBuilder == null) throw new InvalidOperationException($"Set {EntryBuilder} first!");
            return new BuilderAncestorContext(RosterBuilder, ForceBuilder, CategoryBuilder, EntryBuilder, GroupBuilder,
                builder);
        }

        public ICategoryBuilder CategoryBuilder { get; }
        public IEntryBuilder EntryBuilder { get; }
        public IForceBuilder ForceBuilder { get; }
        public IGroupBuilder GroupBuilder { get; }
        public IRosterBuilder RosterBuilder { get; }
        public ISelectionBuilder SelectionBuilder { get; }

        public static BuilderAncestorContext Create(IRosterBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return new BuilderAncestorContext(builder, null, null, null, null, null);
        }
    }
}
