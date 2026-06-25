using FluentAssertions;
using WarHub.ArmouryModel.Source;
using Xunit;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.EditorServices;

public class WhamWorkspaceTests
{
    #region Lifecycle & Catalogue Management (3c-1)

    [Fact]
    public void Create_WithGamesystem_HasCatalogueCompilation()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);

        workspace.CatalogueCompilation.Should().NotBeNull();
        workspace.CatalogueDocumentIds.Should().HaveCount(1);
        workspace.RosterDocumentIds.Should().BeEmpty();
        workspace.Version.Should().Be(0);
    }

    [Fact]
    public void Create_WithMultipleCatalogues_TracksAll()
    {
        var gst = TestData.CreateGamesystem();
        var cat = Catalogue(gst, "MyCatalogue");
        var workspace = WhamWorkspace.Create(gst, cat);

        workspace.CatalogueDocumentIds.Should().HaveCount(2);
    }

    [Fact]
    public void Create_Empty_HasEmptyCompilation()
    {
        var workspace = WhamWorkspace.Create();

        workspace.CatalogueCompilation.Should().NotBeNull();
        workspace.CatalogueDocumentIds.Should().BeEmpty();
    }

    [Fact]
    public void AddCatalogue_ReturnDocumentId_AndUpdatesCatalogueCompilation()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);
        var cat = Catalogue(gst, "AddedCat");

        var catId = workspace.AddCatalogue(cat);

        catId.Id.Should().NotBeEmpty();
        workspace.CatalogueDocumentIds.Should().HaveCount(2);
        workspace.Version.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RemoveCatalogue_RemovesFromWorkspace()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);
        var catId = workspace.AddCatalogue(Catalogue(gst, "ToRemove"));

        workspace.RemoveCatalogue(catId);

        workspace.CatalogueDocumentIds.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveCatalogue_ThrowsForUnknownId()
    {
        var workspace = WhamWorkspace.Create(TestData.CreateGamesystem());

        var act = () => workspace.RemoveCatalogue(DocumentId.CreateNew());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReplaceCatalogue_UpdatesTree()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);
        var catId = workspace.AddCatalogue(Catalogue(gst, "Original"));
        var versionBefore = workspace.Version;

        workspace.ReplaceCatalogue(catId, Catalogue(gst, "Replaced"));

        workspace.Version.Should().BeGreaterThan(versionBefore);
    }

    [Fact]
    public void GetCatalogueTree_ReturnsCatalogueTree()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);
        var docId = workspace.CatalogueDocumentIds[0];

        var tree = workspace.GetCatalogueTree(docId);

        tree.GetRoot().Should().BeOfType<GamesystemNode>();
    }

    [Fact]
    public void GetCatalogueTree_ThrowsForUnknownId()
    {
        var workspace = WhamWorkspace.Create(TestData.CreateGamesystem());

        var act = () => workspace.GetCatalogueTree(DocumentId.CreateNew());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryFindDocumentId_FindsCatalogueByRootNodeId()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);

        var found = workspace.TryFindDocumentId(SourceKind.Gamesystem, gst.Id);

        found.Should().NotBeNull();
        found!.Value.Should().Be(workspace.CatalogueDocumentIds[0]);
    }

    [Fact]
    public void TryFindDocumentId_ReturnsNullForMissing()
    {
        var workspace = WhamWorkspace.Create(TestData.CreateGamesystem());

        var found = workspace.TryFindDocumentId(SourceKind.Catalogue, "nonexistent");

        found.Should().BeNull();
    }

    [Fact]
    public void TryFindDocumentId_ReturnsNullForNullId()
    {
        var workspace = WhamWorkspace.Create(TestData.CreateGamesystem());

        var found = workspace.TryFindDocumentId(SourceKind.Gamesystem, null);

        found.Should().BeNull();
    }

    [Fact]
    public void Version_IncrementsOnEachMutation()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);
        workspace.Version.Should().Be(0);

        var catId = workspace.AddCatalogue(Catalogue(gst, "Cat1"));
        var v1 = workspace.Version;
        v1.Should().BeGreaterThan(0);

        workspace.RemoveCatalogue(catId);
        workspace.Version.Should().BeGreaterThan(v1);
    }

    #endregion

    #region Roster Management (3c-1 continued)

    [Fact]
    public void OpenRoster_WithNode_ReturnsDocumentId()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var roster = CreateMinimalRoster(workspace);

        var rosterId = workspace.OpenRoster(roster);

        rosterId.Id.Should().NotBeEmpty();
        workspace.RosterDocumentIds.Should().HaveCount(1);
    }

    [Fact]
    public void OpenRoster_Empty_CreatesNewRoster()
    {
        var workspace = CreateWorkspaceWithGamesystem();

        var rosterId = workspace.OpenRoster();

        workspace.RosterDocumentIds.Should().HaveCount(1);
        var state = workspace.GetRosterState(rosterId);
        state.Roster.Should().NotBeNull();
    }

    [Fact]
    public void CloseRoster_RemovesFromWorkspace()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();

        workspace.CloseRoster(rosterId);

        workspace.RosterDocumentIds.Should().BeEmpty();
    }

    [Fact]
    public void CloseRoster_ThrowsForUnknownId()
    {
        var workspace = CreateWorkspaceWithGamesystem();

        var act = () => workspace.CloseRoster(DocumentId.CreateNew());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryFindDocumentId_FindsRosterByRootNodeId()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        var rosterNode = workspace.GetRosterState(rosterId).RosterRequired;

        var found = workspace.TryFindDocumentId(SourceKind.Roster, rosterNode.Id);

        found.Should().NotBeNull();
        found!.Value.Should().Be(rosterId);
    }

    #endregion

    #region Multi-Roster Tests (3c-2)

    [Fact]
    public void MultipleRosters_IndependentEditing()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var roster1 = workspace.OpenRoster();
        var roster2 = workspace.OpenRoster();

        workspace.ApplyOperation(roster1, RosterOperations.ChangeRosterName("Roster1"));
        workspace.ApplyOperation(roster2, RosterOperations.ChangeRosterName("Roster2"));

        workspace.GetRosterState(roster1).RosterRequired.Name.Should().Be("Roster1");
        workspace.GetRosterState(roster2).RosterRequired.Name.Should().Be("Roster2");
    }

    [Fact]
    public void MultipleRosters_IndependentUndoRedo()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var roster1 = workspace.OpenRoster();
        var roster2 = workspace.OpenRoster();

        workspace.ApplyOperation(roster1, RosterOperations.ChangeRosterName("R1-Changed"));
        workspace.ApplyOperation(roster2, RosterOperations.ChangeRosterName("R2-Changed"));

        workspace.Undo(roster1).Should().BeTrue();
        workspace.GetRosterState(roster1).RosterRequired.Name.Should().NotBe("R1-Changed");
        workspace.GetRosterState(roster2).RosterRequired.Name.Should().Be("R2-Changed");
    }

    [Fact]
    public void CloseRoster_DoesNotAffectOtherRosters()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var roster1 = workspace.OpenRoster();
        var roster2 = workspace.OpenRoster();
        workspace.ApplyOperation(roster2, RosterOperations.ChangeRosterName("KeepMe"));

        workspace.CloseRoster(roster1);

        workspace.RosterDocumentIds.Should().HaveCount(1);
        workspace.GetRosterState(roster2).RosterRequired.Name.Should().Be("KeepMe");
    }

    [Fact]
    public void MultipleRosters_SharedCatalogueCompilation()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var roster1 = workspace.OpenRoster();
        var roster2 = workspace.OpenRoster();

        var comp1 = workspace.GetRosterCompilation(roster1);
        var comp2 = workspace.GetRosterCompilation(roster2);

        // Both roster compilations reference the same catalogue compilation
        comp1.CatalogueReference.Should().NotBeNull();
        comp2.CatalogueReference.Should().NotBeNull();
        comp1.CatalogueReference.Should().BeSameAs(comp2.CatalogueReference);
    }

    #endregion

    #region Roster Mutations & Undo/Redo (3c-2 continued)

    [Fact]
    public void ApplyOperation_ReturnsNewState()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();

        var newState = workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("NewName"));

        newState.RosterRequired.Name.Should().Be("NewName");
        workspace.GetRosterState(rosterId).RosterRequired.Name.Should().Be("NewName");
    }

    [Fact]
    public void Undo_RestoresPreviousState()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        var originalName = workspace.GetRosterState(rosterId).RosterRequired.Name;
        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("Changed"));

        var result = workspace.Undo(rosterId);

        result.Should().BeTrue();
        workspace.GetRosterState(rosterId).RosterRequired.Name.Should().Be(originalName);
    }

    [Fact]
    public void Undo_ReturnsFalse_WhenAtInitialState()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();

        workspace.Undo(rosterId).Should().BeFalse();
    }

    [Fact]
    public void Redo_ReappliesUndoneOperation()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("Changed"));
        workspace.Undo(rosterId);

        var result = workspace.Redo(rosterId);

        result.Should().BeTrue();
        workspace.GetRosterState(rosterId).RosterRequired.Name.Should().Be("Changed");
    }

    [Fact]
    public void CanUndo_ReflectsEditorState()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();

        workspace.CanUndo(rosterId).Should().BeFalse();
        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("X"));
        workspace.CanUndo(rosterId).Should().BeTrue();
    }

    [Fact]
    public void CanRedo_ReflectsEditorState()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("X"));

        workspace.CanRedo(rosterId).Should().BeFalse();
        workspace.Undo(rosterId);
        workspace.CanRedo(rosterId).Should().BeTrue();
    }

    #endregion

    #region Event & Integration Tests (3c-3)

    [Fact]
    public void ApplyOperation_FiresRosterStateChangedEvent()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        var events = new List<WorkspaceChangedEventArgs>();
        workspace.WorkspaceChanged += (_, e) => events.Add(e);

        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("X"));

        events.Should().ContainSingle()
            .Which.Kind.Should().Be(WorkspaceChangeKind.RosterStateChanged);
        events[0].DocumentId.Should().Be(rosterId);
        events[0].Version.Should().Be(workspace.Version);
    }

    [Fact]
    public void Undo_FiresRosterStateChangedEvent()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("X"));
        var events = new List<WorkspaceChangedEventArgs>();
        workspace.WorkspaceChanged += (_, e) => events.Add(e);

        workspace.Undo(rosterId);

        events.Should().ContainSingle()
            .Which.Kind.Should().Be(WorkspaceChangeKind.RosterStateChanged);
    }

    [Fact]
    public void Redo_FiresRosterStateChangedEvent()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("X"));
        workspace.Undo(rosterId);
        var events = new List<WorkspaceChangedEventArgs>();
        workspace.WorkspaceChanged += (_, e) => events.Add(e);

        workspace.Redo(rosterId);

        events.Should().ContainSingle()
            .Which.Kind.Should().Be(WorkspaceChangeKind.RosterStateChanged);
    }

    [Fact]
    public void Undo_WhenAtInitial_DoesNotFireEvent()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        var events = new List<WorkspaceChangedEventArgs>();
        workspace.WorkspaceChanged += (_, e) => events.Add(e);

        workspace.Undo(rosterId);

        events.Should().BeEmpty();
    }

    [Fact]
    public void OpenRoster_FiresRosterOpenedEvent()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var events = new List<WorkspaceChangedEventArgs>();
        workspace.WorkspaceChanged += (_, e) => events.Add(e);

        var rosterId = workspace.OpenRoster();

        events.Should().ContainSingle()
            .Which.Kind.Should().Be(WorkspaceChangeKind.RosterOpened);
        events[0].DocumentId.Should().Be(rosterId);
    }

    [Fact]
    public void CloseRoster_FiresRosterClosedEvent()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        var events = new List<WorkspaceChangedEventArgs>();
        workspace.WorkspaceChanged += (_, e) => events.Add(e);

        workspace.CloseRoster(rosterId);

        events.Should().ContainSingle()
            .Which.Kind.Should().Be(WorkspaceChangeKind.RosterClosed);
    }

    [Fact]
    public void AddCatalogue_FiresCatalogueAddedEvent()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);
        var events = new List<WorkspaceChangedEventArgs>();
        workspace.WorkspaceChanged += (_, e) => events.Add(e);

        var catId = workspace.AddCatalogue(Catalogue(gst, "New"));

        events.Should().Contain(e => e.Kind == WorkspaceChangeKind.CatalogueAdded);
        events.First(e => e.Kind == WorkspaceChangeKind.CatalogueAdded)
            .DocumentId.Should().Be(catId);
    }

    [Fact]
    public void RemoveCatalogue_FiresCatalogueRemovedEvent()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);
        var catId = workspace.AddCatalogue(Catalogue(gst, "ToRemove"));
        var events = new List<WorkspaceChangedEventArgs>();
        workspace.WorkspaceChanged += (_, e) => events.Add(e);

        workspace.RemoveCatalogue(catId);

        events.Should().Contain(e => e.Kind == WorkspaceChangeKind.CatalogueRemoved);
    }

    [Fact]
    public void ReplaceCatalogue_FiresCatalogueChangedAndCompilationChangedEvents()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);
        var catId = workspace.AddCatalogue(Catalogue(gst, "Original"));
        var events = new List<WorkspaceChangedEventArgs>();
        workspace.WorkspaceChanged += (_, e) => events.Add(e);

        workspace.ReplaceCatalogue(catId, Catalogue(gst, "Replaced"));

        events.Should().Contain(e => e.Kind == WorkspaceChangeKind.CatalogueChanged);
        events.Should().Contain(e => e.Kind == WorkspaceChangeKind.CatalogueCompilationChanged);
    }

    [Fact]
    public void ReplaceCatalogue_ResetsRosterEditors()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);
        var catId = workspace.AddCatalogue(Catalogue(gst, "Original"));
        var rosterId = workspace.OpenRoster();
        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("Edited"));
        workspace.CanUndo(rosterId).Should().BeTrue();

        workspace.ReplaceCatalogue(catId, Catalogue(gst, "Replaced"));

        // Undo history is lost after catalogue change
        workspace.CanUndo(rosterId).Should().BeFalse();
        // Roster still exists with a valid state
        workspace.GetRosterState(rosterId).Roster.Should().NotBeNull();
    }

    [Fact]
    public void ReplaceCatalogue_FiresRosterStateChangedForEachRoster()
    {
        var gst = TestData.CreateGamesystem();
        var workspace = WhamWorkspace.Create(gst);
        var catId = workspace.AddCatalogue(Catalogue(gst, "Original"));
        var roster1 = workspace.OpenRoster();
        var roster2 = workspace.OpenRoster();
        var events = new List<WorkspaceChangedEventArgs>();
        workspace.WorkspaceChanged += (_, e) => events.Add(e);

        workspace.ReplaceCatalogue(catId, Catalogue(gst, "Replaced"));

        var rosterEvents = events.Where(e => e.Kind == WorkspaceChangeKind.RosterStateChanged).ToList();
        rosterEvents.Should().HaveCount(2);
        rosterEvents.Select(e => e.DocumentId).Should().Contain(roster1).And.Contain(roster2);
    }

    [Fact]
    public void EventVersions_AreMonotonicallyIncreasing()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        var versions = new List<long>();
        workspace.WorkspaceChanged += (_, e) => versions.Add(e.Version);

        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("A"));
        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("B"));
        workspace.Undo(rosterId);

        versions.Should().BeInAscendingOrder();
        versions.Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region Diagnostics (3c-3 continued)

    [Fact]
    public void GetDiagnostics_ReturnsDiagnostics()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();

        // Should not throw; diagnostics may be empty for a simple roster
        var diagnostics = workspace.GetDiagnostics(rosterId);
        diagnostics.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDiagnosticsAsync_ReturnsSameAsSync()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();

        var syncDiags = workspace.GetDiagnostics(rosterId);
        var asyncDiags = await workspace.GetDiagnosticsAsync(rosterId);

        asyncDiags.Should().BeEquivalentTo(syncDiags);
    }

    [Fact]
    public async Task GetDiagnosticsAsync_SupportsCancellation()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = () => workspace.GetDiagnosticsAsync(rosterId, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region GetRosterCompilation

    [Fact]
    public void GetRosterCompilation_ReturnsValidCompilation()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();

        var compilation = workspace.GetRosterCompilation(rosterId);

        compilation.Should().NotBeNull();
        compilation.HasCatalogueReference.Should().BeTrue();
        compilation.GlobalNamespace.Rosters.Should().NotBeEmpty();
    }

    [Fact]
    public void GetRosterCompilation_UpdatesAfterEdit()
    {
        var workspace = CreateWorkspaceWithGamesystem();
        var rosterId = workspace.OpenRoster();
        var comp1 = workspace.GetRosterCompilation(rosterId);

        workspace.ApplyOperation(rosterId, RosterOperations.ChangeRosterName("NewName"));
        var comp2 = workspace.GetRosterCompilation(rosterId);

        // After an edit, the compilation should be a new object
        comp2.Should().NotBeSameAs(comp1);
    }

    #endregion

    #region Helpers

    private static WhamWorkspace CreateWorkspaceWithGamesystem()
    {
        return WhamWorkspace.Create(TestData.CreateGamesystem());
    }

    private static RosterNode CreateMinimalRoster(WhamWorkspace workspace)
    {
        // Use the workspace's catalogue compilation to create a roster
        var catState = new RosterState(workspace.CatalogueCompilation);
        var rosterState = RosterOperations.CreateRoster().ApplyTo(catState);
        return rosterState.RosterRequired;
    }

    #endregion
}
