using System.Diagnostics;
using System.Runtime.CompilerServices;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal abstract class SourceDeclaredSymbol : Symbol, INodeDeclaredSymbol<SourceNode>
{
    protected SymbolCompletionState state;
    private ImmutableArray<Symbol> lazyMembers;

    /// <summary>
    /// Thread-local set of symbols currently in <see cref="BindReferences"/>.
    /// Used to detect reentrancy when <see cref="CompilationOptions.DetectBindingReentrancy"/> is enabled.
    /// </summary>
    [ThreadStatic]
    private static HashSet<SourceDeclaredSymbol>? t_bindingSymbols;

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
                case CompletionPart.StartBindingReferences:
                case CompletionPart.FinishBindingReferences:
                    BindReferences();
                    break;
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
                case CompletionPart.StartConstraints:
                case CompletionPart.FinishConstraints:
                    EvaluateConstraints();
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

    protected void BindReferences()
    {
        if (state.HasComplete(CompletionPart.ReferencesCompleted))
        {
            return;
        }
        if (DeclaringCompilation.Options.DetectBindingReentrancy)
        {
            BindReferencesWithReentrancyDetection();
            return;
        }
        BindReferencesCore();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void BindReferencesWithReentrancyDetection()
    {
        var binding = t_bindingSymbols ??= [];
        if (!binding.Add(this))
        {
            ThrowReentrancyDetected(binding);
        }
        try
        {
            BindReferencesCore();
        }
        finally
        {
            binding.Remove(this);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ThrowReentrancyDetected(HashSet<SourceDeclaredSymbol> binding)
    {
        throw new InvalidOperationException(
            $"Binding reentrancy detected on symbol '{Name}' ({GetType().Name}, Id='{Id}'). " +
            $"This symbol is already being bound on this thread. " +
            $"Currently binding: [{string.Join(", ", binding.Select(s => $"{s.GetType().Name}('{s.Name}')"))}]");
    }

    private void BindReferencesCore()
    {
        if (state.NotePartComplete(CompletionPart.StartBindingReferences))
        {
            var diagnostics = BindingDiagnosticBag.GetInstance();
            BindReferencesCore(DeclaringCompilation.GetBinder(Declaration, ContainingSymbol), diagnostics);
            AddDeclarationDiagnostics(diagnostics);
            state.NotePartComplete(CompletionPart.FinishBindingReferences);
        }
        state.SpinWaitComplete(CompletionPart.ReferencesCompleted, default);
    }

    protected virtual void BindReferencesCore(Binder binder, BindingDiagnosticBag diagnostics)
    {
    }

    /// <summary>
    /// Computes effective entries (name/hidden/costs/constraints after modifier application).
    /// Only meaningful for RosterSymbol; base auto-completes.
    /// </summary>
    protected virtual void ComputeEffectiveEntries()
    {
        state.NotePartComplete(CompletionPart.EffectiveEntriesCompleted);
    }

    /// <summary>
    /// Evaluates constraints (min/max selection count, cost limits, etc.).
    /// Only meaningful for RosterSymbol; base auto-completes.
    /// </summary>
    protected virtual void EvaluateConstraints()
    {
        state.NotePartComplete(CompletionPart.ConstraintsCompleted);
    }

    protected T GetBoundField<T>(ref T? field) where T : notnull
    {
        if (!state.HasComplete(CompletionPart.ReferencesCompleted))
        {
            BindReferences();
        }
        return field ?? throw new InvalidOperationException("Bound field was null after binding.");
    }

    protected ImmutableArray<T> GetBoundField<T>(ref ImmutableArray<T> field)
    {
        if (!state.HasComplete(CompletionPart.ReferencesCompleted))
        {
            BindReferences();
        }
        return !field.IsDefault ? field : throw new InvalidOperationException("Bound field still had default value after binding.");
    }


    protected T? GetOptionalBoundField<T>(ref T? field)
    {
        if (!state.HasComplete(CompletionPart.ReferencesCompleted))
        {
            BindReferences();
        }
        return field;
    }
}
