namespace WarHub.ArmouryModel.Source
{
    public interface IRootNode : IIdentifiableNode, INameableNode
    {
        string BattleScribeVersion { get; }
    }

    public partial class CatalogueBaseNode : IRootNode { }

    public partial class RosterNode : IRootNode { }
}
