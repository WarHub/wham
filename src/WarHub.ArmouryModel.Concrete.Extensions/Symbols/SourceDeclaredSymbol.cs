using System.Diagnostics;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal abstract class SourceDeclaredSymbol : Symbol, INodeDeclaredSymbol<SourceNode>
{
    protected SymbolCompletionState state;
    private ImmutableArray<Symbol> lazyMembers;

    protected SourceDeclaredSymbol(
        ISymbol? containingSymbol,
        SourceNode declaration)
    {
        Id = (declaration as IIdentifiableNode)?.Id;
        Name = (declaration as INameableNode)?.Name ?? string.Empty;
        Comment = (declaration as CommentableNode)?.Comment;
        Declaration = declaration;
        ContainingSymbol = containingSymbol;
    }

    public virtual SourceNode Declaration { get; }

    public sealed override ISymbol? ContainingSymbol { get; }

    public override string? Id { get; }

    public override string Name { get; }

    public override string? Comment { get; }

    internal sealed override bool RequiresCompletion => true;

    internal override WhamCompilation DeclaringCompilation
    {
        get
        {
            return base.DeclaringCompilation
                ?? throw new InvalidOperationException("Source symbols must have a declaring compilation set.");
        }
    }

    internal sealed override bool HasComplete(CompletionPart part) => state.HasComplete(part);

    internal override void ForceComplete(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = state.NextIncompletePart;
            switch (incompletePart)
            {
                case CompletionPart.None:
                    return;
                case CompletionPart.Members:
                    GetMembersCore();
                    break;
                case CompletionPart.MembersCompleted:
                    {
                        var members = GetMembersCore();
                        foreach (var member in members)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            member.ForceComplete(cancellationToken);
                        }
                        state.NotePartComplete(CompletionPart.MembersCompleted);
                        break;
                    }
                case CompletionPart.StartEffectiveEntries:
                case CompletionPart.FinishEffectiveEntries:
                    ComputeEffectiveEntries();
                    break;
                case CompletionPart.StartCheckReferences:
                case CompletionPart.FinishCheckReferences:
                    CheckReferences();
                    break;
                case CompletionPart.StartCheckConstraints:
                case CompletionPart.FinishCheckConstraints:
                    CheckConstraints();
                    break;
                default:
                    // This assert will trigger if we forgot to handle any of the completion parts
                    Debug.Assert((incompletePart & CompletionPart.SourceDeclaredSymbolAll) == 0);
                    // any other values are completion parts intended for other kinds of symbols
                    state.NotePartComplete(CompletionPart.All & ~CompletionPart.SourceDeclaredSymbolAll);
                    break;
            }
            state.SpinWaitComplete(incompletePart, cancellationToken);
        }
        throw new InvalidOperationException("Unreachable code.");
    }

    internal sealed override ImmutableArray<ISymbol> GetMembers() => GetMembersCore().Cast<Symbol, ISymbol>();

    protected ImmutableArray<Symbol> GetMembersCore()
    {
        if (state.HasComplete(CompletionPart.Members))
        {
            return lazyMembers!;
        }
        return GetMembersCoreSlow();
    }

    protected ImmutableArray<Symbol> GetMembersCoreSlow()
    {
        if (lazyMembers.IsDefault)
        {
            var diagnostics = BindingDiagnosticBag.GetInstance();
            var members = MakeAllMembers(diagnostics);
            if (ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, members, default).IsDefault)
            {
                AddDeclarationDiagnostics(diagnostics);
                state.NotePartComplete(CompletionPart.Members);
            }
            diagnostics.Free();
        }
        state.SpinWaitComplete(CompletionPart.Members, default);
        return lazyMembers;
    }

    protected virtual ImmutableArray<Symbol> MakeAllMembers(BindingDiagnosticBag diagnostics) =>
        ImmutableArray<Symbol>.Empty;

    /// <summary>
    /// Computes effective entries (name/hidden/costs/constraints after modifier application).
    /// Only meaningful for RosterSymbol; base auto-completes.
    /// </summary>
    protected virtual void ComputeEffectiveEntries()
    {
        state.NotePartComplete(CompletionPart.EffectiveEntriesCompleted);
    }

    /// <summary>
    /// Inspects self-completed bound fields and reports diagnostics for errors.
    /// The base implementation calls <see cref="CheckReferencesCore"/> wrapped in the
    /// Start/Finish completion pattern.
    /// </summary>
    protected void CheckReferences()
    {
        if (state.NotePartComplete(CompletionPart.StartCheckReferences))
        {
            CheckReferencesCore();
            state.NotePartComplete(CompletionPart.FinishCheckReferences);
        }
        state.SpinWaitComplete(CompletionPart.CheckReferencesCompleted, default);
    }

    /// <summary>
    /// Override to access all lazy bound fields, ensuring their binding diagnostics
    /// are reported before <c>GetDiagnostics()</c> returns.
    /// </summary>
    protected virtual void CheckReferencesCore()
    {
    }

    /// <summary>
    /// Evaluates constraints (min/max selection count, cost limits, etc.).
    /// Only meaningful for RosterSymbol; base auto-completes.
    /// </summary>
    protected virtual void CheckConstraints()
    {
        state.NotePartComplete(CompletionPart.CheckConstraintsCompleted);
    }

    /// <summary>
    /// Self-completing bound field accessor. Each field binds itself independently
    /// on first access via <see cref="Interlocked.CompareExchange{T}"/>.
    /// </summary>
    protected T GetBoundField<T>(ref T? field, Func<Binder, BindingDiagnosticBag, T> bind) where T : class
    {
        if (field is not null) return field;
        var binder = DeclaringCompilation.GetBinder(Declaration, ContainingSymbol);
        var diagnostics = BindingDiagnosticBag.GetInstance();
        var result = bind(binder, diagnostics);
        if (Interlocked.CompareExchange(ref field, result, null) is null)
        {
            AddDeclarationDiagnostics(diagnostics);
        }
        diagnostics.Free();
        return field;
    }

    /// <summary>
    /// Self-completing bound field accessor for <see cref="ImmutableArray{T}"/> fields.
    /// </summary>
    protected ImmutableArray<T> GetBoundField<T>(ref ImmutableArray<T> field, Func<Binder, BindingDiagnosticBag, ImmutableArray<T>> bind)
    {
        if (!field.IsDefault) return field;
        var binder = DeclaringCompilation.GetBinder(Declaration, ContainingSymbol);
        var diagnostics = BindingDiagnosticBag.GetInstance();
        var result = bind(binder, diagnostics);
        if (ImmutableInterlocked.InterlockedCompareExchange(ref field, result, default).IsDefault)
        {
            AddDeclarationDiagnostics(diagnostics);
        }
        diagnostics.Free();
        return field;
    }
}
