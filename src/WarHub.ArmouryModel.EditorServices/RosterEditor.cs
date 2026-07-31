namespace WarHub.ArmouryModel.EditorServices;

/// <summary>
/// Provides methods that change roster state. Allows editing roster.
/// Supports undo-redo stack of edits beginning with the initial roster state.
/// </summary>
public sealed class RosterEditor
{
    private ImmutableStack<(RosterState state, IRosterOperation operation)> stateStack
        = ImmutableStack<(RosterState state, IRosterOperation operation)>.Empty;
    private ImmutableStack<(RosterState state, IRosterOperation operation)> redoStack
        = ImmutableStack<(RosterState state, IRosterOperation operation)>.Empty;

    public RosterEditor(RosterState state)
    {
        stateStack = stateStack.Push((state, RosterOperations.Identity));
    }

    public event Action<IRosterOperation, RosterState>? OperationApplied;

    public RosterState State => stateStack.Peek().state;

    public bool CanUndo => !stateStack.Pop().IsEmpty;

    public bool CanRedo => !redoStack.IsEmpty;

    public void ApplyOperation(IRosterOperation operation)
    {
        var newState = operation.Apply(State);
        stateStack = stateStack.Push((newState, operation));
        redoStack = redoStack.Clear();
        OperationApplied?.Invoke(operation, newState);
    }

    public void ApplyOperations(IRosterOperation[] operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        foreach (var op in operations)
        {
            ApplyOperation(op);
        }
    }

    public bool Undo()
    {
        var previousStack = stateStack.Pop(out var current);
        if (previousStack.IsEmpty)
        {
            return false;
        }
        stateStack = previousStack;
        redoStack = redoStack.Push(current);
        return true;
    }

    public bool Redo()
    {
        if (redoStack.IsEmpty)
        {
            return false;
        }
        redoStack = redoStack.Pop(out var redo);
        stateStack = stateStack.Push(redo);
        return true;
    }
}
