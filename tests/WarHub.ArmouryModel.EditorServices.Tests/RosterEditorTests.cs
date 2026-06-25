using FluentAssertions;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.EditorServices;

public class RosterEditorTests
{
    [Fact]
    public void Constructor_InitializesWithGivenState()
    {
        var state = TestData.CreateStateWithRoster();
        var editor = new RosterEditor(state);

        editor.State.Should().BeSameAs(state);
    }

    [Fact]
    public void CanUndo_ReturnsFalse_WhenAtInitialState()
    {
        var editor = new RosterEditor(TestData.CreateStateWithRoster());

        editor.CanUndo.Should().BeFalse();
    }

    [Fact]
    public void CanRedo_ReturnsFalse_WhenAtInitialState()
    {
        var editor = new RosterEditor(TestData.CreateStateWithRoster());

        editor.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void ApplyOperation_ChangesState()
    {
        var state = TestData.CreateStateWithRoster();
        var editor = new RosterEditor(state);
        var renameOp = RosterOperations.ChangeRosterName("New Name");

        editor.ApplyOperation(renameOp);

        editor.State.RosterRequired.Name.Should().Be("New Name");
        editor.CanUndo.Should().BeTrue();
    }

    [Fact]
    public void Undo_RestoresPreviousState()
    {
        var state = TestData.CreateStateWithRoster();
        var editor = new RosterEditor(state);
        editor.ApplyOperation(RosterOperations.ChangeRosterName("Changed"));

        var result = editor.Undo();

        result.Should().BeTrue();
        editor.State.Should().BeSameAs(state);
        editor.CanUndo.Should().BeFalse();
        editor.CanRedo.Should().BeTrue();
    }

    [Fact]
    public void Undo_ReturnsFalse_WhenAtInitialState()
    {
        var editor = new RosterEditor(TestData.CreateStateWithRoster());

        editor.Undo().Should().BeFalse();
    }

    [Fact]
    public void Redo_ReappliesUndoneOperation()
    {
        var state = TestData.CreateStateWithRoster();
        var editor = new RosterEditor(state);
        editor.ApplyOperation(RosterOperations.ChangeRosterName("Changed"));
        editor.Undo();

        var result = editor.Redo();

        result.Should().BeTrue();
        editor.State.RosterRequired.Name.Should().Be("Changed");
        editor.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void Redo_ReturnsFalse_WhenNoUndoneOperations()
    {
        var editor = new RosterEditor(TestData.CreateStateWithRoster());

        editor.Redo().Should().BeFalse();
    }

    [Fact]
    public void ApplyOperation_ClearsRedoStack()
    {
        var editor = new RosterEditor(TestData.CreateStateWithRoster());
        editor.ApplyOperation(RosterOperations.ChangeRosterName("First"));
        editor.Undo();
        editor.CanRedo.Should().BeTrue();

        editor.ApplyOperation(RosterOperations.ChangeRosterName("Second"));

        editor.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void ApplyOperation_FiresOperationAppliedEvent()
    {
        var editor = new RosterEditor(TestData.CreateStateWithRoster());
        IRosterOperation? firedOp = null;
        RosterState? firedState = null;
        editor.OperationApplied += (op, state) => { firedOp = op; firedState = state; };
        var renameOp = RosterOperations.ChangeRosterName("Evented");

        editor.ApplyOperation(renameOp);

        firedOp.Should().BeSameAs(renameOp);
        firedState.Should().BeSameAs(editor.State);
    }

    [Fact]
    public void ApplyOperations_AppliesMultipleInSequence()
    {
        var editor = new RosterEditor(TestData.CreateStateWithRoster());
        var ops = new IRosterOperation[]
        {
            RosterOperations.ChangeRosterName("Step1"),
            RosterOperations.ChangeRosterName("Step2"),
        };

        editor.ApplyOperations(ops);

        editor.State.RosterRequired.Name.Should().Be("Step2");
    }

    [Fact]
    public void ApplyOperations_WithNull_Throws()
    {
        var state = TestData.CreateStateWithRoster();
        var editor = new RosterEditor(state);

        var act = () => editor.ApplyOperations(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MultipleUndoRedo_TraversesFullHistory()
    {
        var editor = new RosterEditor(TestData.CreateStateWithRoster());
        editor.ApplyOperation(RosterOperations.ChangeRosterName("A"));
        editor.ApplyOperation(RosterOperations.ChangeRosterName("B"));
        editor.ApplyOperation(RosterOperations.ChangeRosterName("C"));

        editor.State.RosterRequired.Name.Should().Be("C");
        editor.Undo().Should().BeTrue();
        editor.State.RosterRequired.Name.Should().Be("B");
        editor.Undo().Should().BeTrue();
        editor.State.RosterRequired.Name.Should().Be("A");
        editor.Undo().Should().BeTrue();
        editor.Undo().Should().BeFalse();
        editor.Redo().Should().BeTrue();
        editor.State.RosterRequired.Name.Should().Be("A");
    }

    [Fact]
    public void Identity_DoesNotChangeState()
    {
        var state = TestData.CreateStateWithRoster();
        var editor = new RosterEditor(state);

        editor.ApplyOperation(RosterOperations.Identity);

        editor.State.RosterRequired.Name.Should().Be(state.RosterRequired.Name);
    }
}
