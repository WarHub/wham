namespace WarHub.ArmouryModel.Workspaces.BattleScribe
{
    public record XmlWorkspaceOptions
    {
        public string SourceDirectory { get; init; } = ".";

        public bool IncludeUnknown { get; init; }
    }
}
