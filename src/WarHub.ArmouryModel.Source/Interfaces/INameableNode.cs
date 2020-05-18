namespace WarHub.ArmouryModel.Source
{
    public interface INameableNode
    {
        string? Name { get; }
    }

    public partial class CatalogueBaseNode : INameableNode { }
    public partial class CatalogueLinkNode : INameableNode { }
    public partial class CharacteristicNode : INameableNode { }
    public partial class CharacteristicTypeNode : INameableNode { }
    public partial class CostBaseNode : INameableNode { }
    public partial class CostTypeNode : INameableNode { }
    public partial class DataIndexNode : INameableNode { }
    public partial class EntryBaseNode : INameableNode { }
    public partial class ProfileTypeNode : INameableNode { }
    public partial class PublicationNode : INameableNode { }
    public partial class RosterElementBaseNode : INameableNode { }
    public partial class RosterNode : INameableNode { }
}
