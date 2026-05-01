namespace WarHub.ArmouryModel.EditorServices;

/// <summary>
/// Stable identity for a source tree (catalogue, gamesystem, or roster) in a <see cref="WhamWorkspace"/>.
/// Synthetic GUID assigned at load time — not derived from BattleScribe root node IDs.
/// </summary>
public readonly record struct DocumentId(Guid Id)
{
    public static DocumentId CreateNew() => new(Guid.NewGuid());
}
