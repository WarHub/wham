namespace WarHub.ArmouryModel.EditorServices;

/// <summary>
/// Describes the kind of change that occurred in a <see cref="WhamWorkspace"/>.
/// </summary>
public enum WorkspaceChangeKind
{
    CatalogueAdded,
    CatalogueRemoved,
    CatalogueChanged,
    CatalogueCompilationChanged,
    RosterOpened,
    RosterClosed,
    RosterStateChanged,
}
