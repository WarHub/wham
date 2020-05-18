namespace WarHub.ArmouryModel.Source
{
    public interface IRootNode : INameableNode
    {
        string? BattleScribeVersion { get; }
    }

    public partial class CatalogueBaseNode : IRootNode { }

    public partial class DataIndexNode : IRootNode { }

    public partial class RosterNode : IRootNode { }
}
