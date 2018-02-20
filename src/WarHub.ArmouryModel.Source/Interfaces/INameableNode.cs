namespace WarHub.ArmouryModel.Source
{
    public interface INameableNode
    {
        string Name { get; }
    }

    partial class CatalogueBaseNode : INameableNode { }
    partial class RosterElementBaseNode : INameableNode { }
    partial class EntryBaseNode : INameableNode { }
    partial class CharacteristicNode : INameableNode { }
    partial class CharacteristicTypeNode : INameableNode { }
    partial class CostBaseNode : INameableNode { }
    partial class CostTypeNode : INameableNode { }
    partial class DataIndexNode : INameableNode { }
    partial class ProfileTypeNode : INameableNode { }
    partial class RosterNode : INameableNode { }
}
