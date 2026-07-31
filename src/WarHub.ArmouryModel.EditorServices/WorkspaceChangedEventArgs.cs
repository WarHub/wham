namespace WarHub.ArmouryModel.EditorServices;

/// <summary>
/// Event data for <see cref="WhamWorkspace.WorkspaceChanged"/> events.
/// Carries the workspace version at the time of the change so consumers can detect stale events.
/// </summary>
public sealed class WorkspaceChangedEventArgs(
    WorkspaceChangeKind kind,
    DocumentId documentId,
    long version) : EventArgs
{
    public WorkspaceChangeKind Kind { get; } = kind;

    public DocumentId DocumentId { get; } = documentId;

    /// <summary>
    /// The workspace version after this change was applied.
    /// Monotonically increasing; consumers can use this to detect stale events.
    /// </summary>
    public long Version { get; } = version;
}
